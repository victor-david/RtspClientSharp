using System;

namespace SimpleRtspPlayer.RawFramesDecoding.DecodedFrames
{
    public interface IDecodedVideoFrame
    {
        DecodedVideoFrameParameters FrameParameters { get; }
        void TransformTo(IntPtr buffer, int bufferStride, TransformParameters transformParameters);
    }
}