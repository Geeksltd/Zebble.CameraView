namespace Zebble.Plugin
{
    using Zebble;

    public class CameraView : View, IRenderedBy<Renderer.CameraViewRenderer>
    {
        public CameraView() => this.Size(100.Percent());
    }
}