namespace Zebble
{
    using System.Threading.Tasks;
    using UIKit;

    class CameraViewRenderer : INativeRenderer
    {
        IosCameraView Result;

        public Task<UIView> Render(Renderer renderer)
        {
            Result = new IosCameraView((CameraView)renderer.View);
            return Task.FromResult<UIView>(Result);
        }

        public void Dispose()
        {
            Result?.Dispose();
            Result = null;
        }
    }
}