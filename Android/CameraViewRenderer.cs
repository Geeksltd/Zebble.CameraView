namespace Zebble.Plugin.Renderer
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Zebble;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CameraViewRenderer : ICustomRenderer
    {
        Plugin.CameraView View;
        AndroidCameraView Result;

        public async Task<Android.Views.View> Render(object view)
        {
            View = (Plugin.CameraView)view;
            Result = new AndroidCameraView(View);

            return Result;
        }

        public void Dispose() => Result.Dispose();
    }
}