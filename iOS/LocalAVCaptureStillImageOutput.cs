namespace Zebble
{
    using System;
    using System.Threading.Tasks;
    using AVFoundation;
    using CoreGraphics;
    using CoreMedia;
    using CoreVideo;
    using UIKit;
    using Olive;

    public class LocalAVCaptureStillImageOutput : AVCaptureVideoDataOutputSampleBufferDelegate
    {
        CameraView Camera;
        byte[] CurrentImageBuffer;
        DateTime LastFrameCaptured;

        public LocalAVCaptureStillImageOutput(CameraView camera)
        {
            Camera = camera;
            camera.RequestFrameCapture += CameraOnRequestFrameCapture;
            LastFrameCaptured = LocalTime.Now;
        }

        public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            if (connection.SupportsVideoOrientation)
                connection.VideoOrientation = GetVideoOrientation();

            //if (DateTime.Now < LastFrameCaptured.AddSeconds(0.1)) return; // capture 10 frames per second
            //LastFrameCaptured = DateTime.Now;

            Log.For(this).Debug("############ ACTION: capturing picture");

            CurrentImageBuffer = GetBytes(sampleBuffer.GetImageBuffer() as CVPixelBuffer);
            sampleBuffer.Dispose();
        }

        AVCaptureVideoOrientation GetVideoOrientation()
        {
            return UIApplication.SharedApplication.StatusBarOrientation switch
            {
                UIInterfaceOrientation.PortraitUpsideDown => AVCaptureVideoOrientation.PortraitUpsideDown,
                UIInterfaceOrientation.LandscapeLeft => AVCaptureVideoOrientation.LandscapeLeft,
                UIInterfaceOrientation.LandscapeRight => AVCaptureVideoOrientation.LandscapeRight,
                _ => AVCaptureVideoOrientation.Portrait,
            };
        }

        void CameraOnRequestFrameCapture(TaskCompletionSource<byte[]> source)
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

            Log.For(this).Debug(baseAddress + " " + bytesPerRow + " " + width + " " + height);

            var flags = CGBitmapFlags.PremultipliedFirst | CGBitmapFlags.ByteOrder32Little;

            // Create a CGImage on the RGB colorspace from the configured parameter above
            using var cs = CGColorSpace.CreateDeviceRGB();
            using var context = new CGBitmapContext(baseAddress, width, height, 8, bytesPerRow, cs, flags);
            using var cgImage = context.ToImage();

            pixelBuffer.Unlock(CVPixelBufferLock.None);
            var capturedImage = UIImage.FromImage(cgImage);

            return capturedImage.AsPNG().ToArray();
        }
    }
}