using System;

namespace SimpleRtspPlayer.RawFramesDecoding.DecodedFrames
{
    class DecodedVideoFrame : IDecodedVideoFrame
    {
        private readonly Action<IntPtr, int, TransformParameters> transformAction;
        public DecodedVideoFrameParameters FrameParameters { get; }

        public DecodedVideoFrame(Action<IntPtr, int, TransformParameters> transformAction, DecodedVideoFrameParameters frameParameters)
        {
            this.transformAction = transformAction ?? throw new ArgumentNullException(nameof(transformAction));
            FrameParameters = frameParameters ?? throw new ArgumentNullException(nameof(frameParameters));
        }

        public void TransformTo(IntPtr buffer, int bufferStride, TransformParameters transformParameters)
        {
            transformAction(buffer, bufferStride, transformParameters);
        }
    }
}