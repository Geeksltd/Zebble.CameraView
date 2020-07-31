namespace Zebble
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using AVFoundation;
    using CoreFoundation;
    using CoreGraphics;
    using CoreMedia;
    using CoreVideo;
    using Foundation;
    using UIKit;

    public class LocalAVCaptureStillImageOutput : AVFoundation.AVCaptureVideoDataOutputSampleBufferDelegate
    {
        CameraView Camera;
        byte[] CurrentImageBuffer;
        DateTime LastFrameCaptured;

        public LocalAVCaptureStillImageOutput(CameraView camera)
        {
            Camera = camera;
            camera.RequestFrameCapture += Camera_RequestFrameCapture;
            LastFrameCaptured = DateTime.Now;
        }

        public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            if (connection.SupportsVideoOrientation)
                connection.VideoOrientation = GetVideoOrientation();

            if (DateTime.Now < LastFrameCaptured.AddSeconds(0.1)) return; // capture 10 frames per second
            LastFrameCaptured = DateTime.Now;

            Device.Log.Message("############ ACTION: capturing picture");

            CurrentImageBuffer = GetBytes(sampleBuffer.GetImageBuffer() as CVPixelBuffer);
            sampleBuffer.Dispose();            
        }

        AVCaptureVideoOrientation GetVideoOrientation()
        {
            switch (UIApplication.SharedApplication.StatusBarOrientation)
            {   
                case UIInterfaceOrientation.PortraitUpsideDown:
                    return AVCaptureVideoOrientation.PortraitUpsideDown;
                case UIInterfaceOrientation.LandscapeLeft:
                    return AVCaptureVideoOrientation.LandscapeLeft;
                case UIInterfaceOrientation.LandscapeRight:
                    return AVCaptureVideoOrientation.LandscapeRight;
                default:
                    return AVCaptureVideoOrientation.Portrait;
            }
        }

        void Camera_RequestFrameCapture(TaskCompletionSource<byte[]> source)
        {
            var buffer = CurrentImageBuffer ?? new byte[0];            
            source.TrySetResult(buffer);            
        }
        byte[] GetBytes(CVPixelBuffer pixelBuffer)
        {
            pixelBuffer.Lock(CVPixelBufferLock.None);

            var baseAddress = pixelBuffer.BaseAddress;
            var bytesPerRow = pixelBuffer.BytesPerRow;
            var width = pixelBuffer.Width;
            var height = pixelBuffer.Height;
            var log = baseAddress + " " + bytesPerRow + " " + width + " " + height;
            Zebble.Device.Log.Message(log);
            
            var flags = CGBitmapFlags.PremultipliedFirst | CGBitmapFlags.ByteOrder32Little;
            // Create a CGImage on the RGB colorspace from the configured parameter above
            using (var cs = CGColorSpace.CreateDeviceRGB())
            using (var context = new CGBitmapContext(baseAddress, width, height, 8, bytesPerRow, cs, flags))
            using (var cgImage = context.ToImage())
            {
                pixelBuffer.Unlock(CVPixelBufferLock.None);
                var capturedImage = UIImage.FromImage(cgImage);
                 return capturedImage.AsPNG().ToArray();
            }
            
        }
    }

    class IosCameraView : UIView
    {
        CameraView Camera;
        AVCaptureSession CaptureSession;
        LocalAVCaptureStillImageOutput Receiver;

        public IosCameraView(View view)
        {
            Camera = (CameraView)view;

            Camera.WhenShown(() => Thread.UI.Run(SetupLiveCameraStream));
            Receiver = new LocalAVCaptureStillImageOutput(Camera);
        }

        public async Task SetupLiveCameraStream()
        {
            var authorised = await AuthorizeCameraUse();

            if (!authorised) return;

            var captureDevice = CreateCaptureDevice();
            if (captureDevice == null) return;
            
            CaptureSession = new AVCaptureSession();
            CaptureSession.BeginConfiguration();
            CaptureSession.SessionPreset = AVCaptureSession.Preset640x480;
            CaptureSession.AddInput(AVCaptureDeviceInput.FromDevice(captureDevice));
            CaptureSession.AddOutput(CreateVideoDataOutput());
            CaptureSession.CommitConfiguration();
            CaptureSession.StartRunning();

            if (!Camera.DisablePreview)
                Layer.AddSublayer(new AVCaptureVideoPreviewLayer(CaptureSession)
                {
                    Frame = Frame,
                    VideoGravity = AVLayerVideoGravity.Resize
                });
        }

        private AVCaptureOutput CreateVideoDataOutput()
        {
            var result = new AVCaptureVideoDataOutput 
            { 
                AlwaysDiscardsLateVideoFrames = true ,
                WeakVideoSettings = new CVPixelBufferAttributes { PixelFormatType = CVPixelFormatType.CV32BGRA }.Dictionary
            };
            
            result.SetSampleBufferDelegateQueue(Receiver, DispatchQueue.MainQueue);
            
            return result;
        }

        async Task<bool> AuthorizeCameraUse()
        {
            var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);

            if (authorizationStatus != AVAuthorizationStatus.Authorized)
                return await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);

            return true;
        }

        AVCaptureDevice CreateCaptureDevice()
        {
            var device = 
                AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera,AVMediaTypes.Video, GetCameraPosition());
            
            if (device != null)
            {
                var error = new NSError();
                if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
                {
                    device.LockForConfiguration(out error);
                    device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
                    device.UnlockForConfiguration();
                }
                else if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
                {
                    device.LockForConfiguration(out error);
                    device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
                    device.UnlockForConfiguration();
                }
                else if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
                {
                    device.LockForConfiguration(out error);
                    device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
                    device.UnlockForConfiguration();
                }               
                
            }
            
            return device;
        }

        private AVCaptureDevicePosition GetCameraPosition()
        {
            switch (Camera.CameraPosition)
            {
                case CameraPosition.Back:
                    return AVCaptureDevicePosition.Back;
                default:
                    return AVCaptureDevicePosition.Front;
            }
             
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Device.Log.Message("############ ACTION: Stopping the camera");
                CaptureSession?.StopRunning();
                Camera = null;
            }

            base.Dispose(disposing);
        }
    }
}