namespace Zebble.Plugin.Renderer
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using UIKit;
    using Zebble;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CameraViewRenderer : INativeRenderer
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