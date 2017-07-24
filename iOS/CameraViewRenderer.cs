namespace Zebble.Plugin.Renderer
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Zebble;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CameraViewRenderer : ICustomRenderer
    {
        CameraView View;
        IosCameraView Result;

        public async Task<UIKit.UIView> Render(object view)
        {
            View = (CameraView)view;
            Result = new IosCameraView(View);

            return Result;
        }

        public void Dispose() => Result.Dispose();
    }
}