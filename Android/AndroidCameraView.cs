namespace Zebble
{
    using Android.Hardware;
    using Android.Views;
    using Android.Widget;
    using Olive;

    class AndroidCameraView : FrameLayout, ISurfaceHolderCallback
    {
        CameraView View;
        Camera PreviewCamera;
        SurfaceView SurfaceView;
        ISurfaceHolder Holder;

        public AndroidCameraView(CameraView view) : base(UIRuntime.CurrentActivity)
        {
            View = view;
            Create();
            Start();
        }

        void Create()
        {
            SurfaceView = new SurfaceView(UIRuntime.CurrentActivity);
            AddView(SurfaceView);

            Holder = SurfaceView.Holder;
            Holder.AddCallback(this);
            Holder.SetType(SurfaceType.PushBuffers);
        }

        public void Start()
        {
            PreviewCamera = Camera.Open();
            PreviewCamera.SetDisplayOrientation(90);
            PreviewCamera.StartPreview();
        }

        public void Pause()
        {
            PreviewCamera.StopPreview();
            if (PreviewCamera == null) return;

            PreviewCamera = null;
            PreviewCamera.Release();
            PreviewCamera = null;
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try { PreviewCamera?.SetPreviewDisplay(holder); }
            catch (Java.IO.IOException exception)
            {
                Log.For(this).Error(exception, "IOException caused by setPreviewDisplay().");
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder) => PreviewCamera?.StopPreview();

        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
        {
            //Do rotation if needed
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PreviewCamera?.Dispose();
                Holder?.Dispose();
                SurfaceView?.Dispose();

                PreviewCamera = null;
                Holder = null;
                SurfaceView = null;
                View = null;
            }

            base.Dispose(disposing);
        }
    }
}