namespace Zebble.Plugin.Renderer
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using AVFoundation;
    using Foundation;
    using UIKit;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IosCameraView : UIView
    {
        Plugin.CameraView Camera;
        AVCaptureSession CaptureSession;

        public IosCameraView(View view)
        {
            Camera = (Plugin.CameraView)view;
            Camera.WhenShown(() => Device.UIThread.Run(SetupLiveCameraStream));
        }

        public async Task SetupLiveCameraStream()
        {
            var authorised = await AuthorizeCameraUse();

            if (!authorised) return;

            var captureDevice = CreateCaptureDevice();
            if (captureDevice == null) return;

            CaptureSession = new AVCaptureSession();
            CaptureSession.AddInput(AVCaptureDeviceInput.FromDevice(captureDevice));
            CaptureSession.AddOutput(new AVCaptureStillImageOutput { OutputSettings = new NSDictionary() });
            CaptureSession.StartRunning();

            Layer.AddSublayer(new AVCaptureVideoPreviewLayer(CaptureSession)
            {
                Frame = Frame,
                VideoGravity = AVLayerVideoGravity.Resize
            });
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
            var device = AVCaptureDevice.DefaultDeviceWithMediaType(AVMediaType.Video);

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CaptureSession?.StopRunning();
                Camera = null;
            }

            base.Dispose(disposing);
        }
    }
}