using SimpleRtspPlayer.RawFramesDecoding;
using SimpleRtspPlayer.RawFramesDecoding.DecodedFrames;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PixelFormat = SimpleRtspPlayer.RawFramesDecoding.PixelFormat;

namespace SimpleRtspPlayer.GUI.Views
{
    /// <summary>
    /// Interaction logic for VideoView.xaml
    /// </summary>
    public partial class VideoView
    {
        #region Private
        private WriteableBitmap writeableBitmap;
        private Int32Rect dirtyRect;
        private TransformParameters transformParameters;
        private readonly Action<IDecodedVideoFrame> invalidateAction;
        #endregion

        #region VideoSource
        public IVideoSource VideoSource
        {
            get => (IVideoSource)GetValue(VideoSourceProperty);
            set => SetValue(VideoSourceProperty, value);
        }

        public static readonly DependencyProperty VideoSourceProperty = DependencyProperty.Register
            (
                nameof(VideoSource), typeof(IVideoSource), typeof(VideoView), new FrameworkPropertyMetadata(OnVideoSourceChanged)
            );


        private static void OnVideoSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VideoView view)
            {
                if (e.OldValue is IVideoSource oldVideoSource)
                {
                    oldVideoSource.FrameReceived -= view.OnFrameReceived;
                }

                if (e.NewValue is IVideoSource newVideoSource)
                {
                    newVideoSource.FrameReceived += view.OnFrameReceived;
                }
                view.writeableBitmap = null;
            }
        }
        #endregion

        #region Constructor
        public VideoView()
        {
            InitializeComponent();
            invalidateAction = Invalidate;
        }
        #endregion

        #region Private methods
        private void OnFrameReceived(object sender, IDecodedVideoFrame decodedFrame)
        {
            Application.Current.Dispatcher.Invoke(invalidateAction, DispatcherPriority.Send, decodedFrame);
        }

        private void Invalidate(IDecodedVideoFrame decodedVideoFrame)
        {
            /* will be null on first frame received */
            if (writeableBitmap == null)
            {
                InitializeBitmap(decodedVideoFrame.FrameParameters);
                dirtyRect = new Int32Rect(0, 0, decodedVideoFrame.FrameParameters.Width, decodedVideoFrame.FrameParameters.Height);
            }

            writeableBitmap.Lock();

            try
            {
                decodedVideoFrame.TransformTo(writeableBitmap.BackBuffer, writeableBitmap.BackBufferStride, transformParameters);
                writeableBitmap.AddDirtyRect(dirtyRect);
            }
            finally
            {
                writeableBitmap.Unlock();
                VideoImage.Source = writeableBitmap;
            }
        }

        private void InitializeBitmap(DecodedVideoFrameParameters parms)
        {
            transformParameters = new TransformParameters
                (
                    RectangleF.Empty,
                    new System.Drawing.Size(parms.Width, parms.Height),
                    ScalingPolicy.RespectAspectRatio, PixelFormat.Bgra32, ScalingQuality.FastBilinear
                );

            writeableBitmap = new WriteableBitmap
                (
                    parms.Width, parms.Height, ScreenInfo.DpiX, ScreenInfo.DpiY, PixelFormats.Pbgra32, null
                );
            RenderOptions.SetBitmapScalingMode(writeableBitmap, BitmapScalingMode.NearestNeighbor);

            VideoImage.Source = writeableBitmap;
        }
        #endregion
    }
}