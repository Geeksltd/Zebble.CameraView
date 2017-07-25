namespace Zebble.Plugin.Renderer
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Zebble;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CameraViewRenderer : INativeRenderer
    {
        AndroidCameraView Result;

        public Task<Android.Views.View> Render(Renderer renderer)
        {
            Result = new AndroidCameraView((CameraView)renderer.View);

            return Task.FromResult<Android.Views.View>(Result);
        }

        public void Dispose()
        {
            Result?.Dispose();
            Result = null;
        }
    }
}