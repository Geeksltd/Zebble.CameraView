namespace Zebble.Plugin
{
    using Zebble;

    public class CameraView : CustomRenderedView<Renderer.CameraViewRenderer>
    {
        public CameraView() => this.Size(100.Percent());
    }
}