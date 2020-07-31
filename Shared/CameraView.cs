using System;
using System.Threading.Tasks;
namespace Zebble
{
    public class CameraView : View, IRenderedBy<CameraViewRenderer>
    {
        public bool DisablePreview { get; set; }
        public CameraPosition CameraPosition { get; set; } = CameraPosition.Back;

        public CameraView()=> 
            this.Size(100.Percent());
                
        internal event Action<TaskCompletionSource<byte[] >> RequestFrameCapture;

        public Task< byte[] > CaptureFrame()
        {
            var source = new TaskCompletionSource<byte[]>();
            RequestFrameCapture?.Invoke(source);
            return source.Task;
        }
    }

}