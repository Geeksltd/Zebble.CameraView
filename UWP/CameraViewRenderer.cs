namespace Zebble
{
    using System;
    using System.Threading.Tasks;
    using Windows.Graphics.Display;
    using Windows.Media.Capture;
    using Windows.System.Display;
    using Windows.UI.Xaml;
    using controls = Windows.UI.Xaml.Controls;

    class CameraViewRenderer : INativeRenderer
    {
        controls.CaptureElement Result;
        MediaCapture Capture;
        bool IsPreviewing;
        DisplayRequest DisplayRequest;

        public async Task<FrameworkElement> Render(Renderer _)
        {
            Result = new controls.CaptureElement { Stretch = Windows.UI.Xaml.Media.Stretch.UniformToFill };

            DisplayRequest = new DisplayRequest();

            try
            {
                Capture = new MediaCapture();

                await Capture.InitializeAsync().AsTask();

                Capture.SetPreviewRotation(VideoRotation.Clockwise90Degrees);

                Result.Source = Capture;

                await Capture.StartPreviewAsync().AsTask();
                IsPreviewing = true;

                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }
            catch (UnauthorizedAccessException)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                await Alert.Toast("The app was denied access to the camera");
            }
            catch (Exception ex)
            {
                await Alert.Toast("MediaCapture initialization failed: " + ex.Message);
            }

            return Result;
        }

        async Task CleanupCameraAsync()
        {
            if (Capture != null)
            {
                if (IsPreviewing)
                {
                    await Capture.StopPreviewAsync();
                }

                Device.UIThread.RunAction(() =>
                {
                    Result.Source = null;
                    DisplayRequest?.RequestRelease();

                    Capture.Dispose();
                    Capture = null;
                });
            }
        }

        public void Dispose()
        {
            if (Capture == null) return;

            if (IsPreviewing)
                Capture.StopPreviewAsync().AsTask().ContinueWith(x =>
                {
                    Capture.Dispose();
                    Capture = null;
                });

            Result.Source = null;
            Result = null;
        }
    }
}