namespace Zebble
{
    public class CameraView : View, IRenderedBy<CameraViewRenderer>
    {
        public CameraView() => this.Size(100.Percent());
    }
}