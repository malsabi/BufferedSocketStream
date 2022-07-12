using System.Collections.Concurrent;
using BufferedSocketStream.Enums;

namespace BufferedSocketStream.Common.DataManagement
{
    internal class DataManagerBase : IDisposable
    {
        #region "Properties"
        protected int MaximumMessageSize { get; set; }

        protected byte[] Message { get; set; }

        protected int MessageSize { get; set; }

        protected int HeaderSize { get; set; }

        protected int BufferSize { get; set; }

        protected int ReadOffset { get; set; }

        protected int WriteOffset { get; set; }

        protected bool IsBuffering { get; set; }

        protected object IsBufferingLock { get; set; }

        protected byte[] HeaderBuffer { get; set; }

        protected byte[] MessageBuffer { get; set; }

        protected BufferState BufferState { get; set; }

        protected ConcurrentQueue<byte[]> BufferQueue { get; set; }
        #endregion

        public DataManagerBase()
        {
            IsBufferingLock = new object();
            BufferQueue = new ConcurrentQueue<byte[]>();
        }

        public void Dispose()
        {
            if (Message != null)
            {
                Message = null;
            }
            if (HeaderBuffer != null)
            {
                HeaderBuffer = null;
            }
            if (MessageBuffer != null)
            {
                MessageBuffer = null;
            }
            if (BufferQueue != null)
            {
                BufferQueue.Clear();
                BufferQueue = null;
            }
            GC.Collect();
        }
    }
}