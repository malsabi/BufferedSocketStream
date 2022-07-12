using System.Net.Sockets;

namespace BufferedSocketStream.Common.DataManagement
{
    internal class ReceiveDataManager : DataManagerBase
    {
        #region "Properties"
        public ConnectionConfiguration Configuration { get; private set; }
        public ConnectionInfo Connection { get; private set; }
        public SocketAsyncEventArgs ReceiveAsyncEventArgs { get; private set; }
        #endregion

        public ReceiveDataManager(ConnectionConfiguration Configuration, ConnectionInfo Connection, SocketAsyncEventArgs ReceiveAsyncEventArgs) : base()
        {
            this.Configuration = Configuration;
            this.Connection = Connection;
            this.ReceiveAsyncEventArgs = ReceiveAsyncEventArgs;
            Initialize();
        }

        #region "Internal Methods"
        internal void HandleReceiveAsync()
        {
            if (Connection != null && Connection.IsClosed)
            {
                return;
            }
            if (Connection.Handle == null)
            {
                return;
            }
            if (Connection.Handle.ReceiveAsync(ReceiveAsyncEventArgs) == false)
            {
                ProcessReceiveAsync(ReceiveAsyncEventArgs);
            }
        }

        internal void ClearBufferQueue()
        {
            IsBuffering = false;
            Dispose();
        }
        #endregion

        #region "Private Methods"
        private void Initialize()
        {
            ReceiveAsyncEventArgs.Completed += OnReceiveAsyncCompleted;
            MaximumMessageSize = Configuration.MaximumMessageSize;
            HeaderSize = Configuration.HeaderSize;
            BufferSize = Configuration.BufferSize;

            HeaderBuffer = new byte[HeaderSize];
            MessageBuffer = null;
            BufferState = Enums.BufferState.Header;
        }

        private void ProcessReceiveAsync(SocketAsyncEventArgs e)
        {
            if (Connection != null && Connection.IsClosed)
            {
                return;
            }

            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0 && e.Buffer != null)
                {
                    byte[] Packet = new byte[e.BytesTransferred];
                    Buffer.BlockCopy(e.Buffer, 0, Packet, 0, e.BytesTransferred);
                    Producer(Packet);
                    HandleReceiveAsync();
                }
            }
            else
            {
                Connection.Dispose();
            }
        }

        private void OnReceiveAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceiveAsync(e);
                    break;
                default:
                    Connection.Dispose();
                    break;
            }
        }
        #endregion


        #region "Producer-Consumer Buffering"
        private void Producer(byte[] Packet)
        {
            if (BufferQueue == null || IsBufferingLock == null)
            {
                return;
            }
            BufferQueue.Enqueue(Packet);
            lock (IsBufferingLock)
            {
                if (IsBuffering == false)
                {
                    IsBuffering = true;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Consumer), null);
                }
            }
        }

        private void Consumer(object o)
        {
            while (IsBuffering)
            {
                if (BufferQueue.IsEmpty)
                {
                    lock (IsBufferingLock)
                    {
                        IsBuffering = false;
                    }
                    break;
                }
                BufferQueue.TryDequeue(out byte[] bufferChunk);

                if (bufferChunk == null)
                {
                    return;
                }

                int numberOfBytesToProcess = bufferChunk.Length;
                while (numberOfBytesToProcess > 0)
                {
                    switch (BufferState)
                    {
                        case Enums.BufferState.Header:
                            if (numberOfBytesToProcess + WriteOffset >= HeaderSize)
                            {
                                int headerBytesLength = (numberOfBytesToProcess >= HeaderSize) ? HeaderSize - WriteOffset : numberOfBytesToProcess;
                                Buffer.BlockCopy(bufferChunk, ReadOffset, HeaderBuffer, WriteOffset, headerBytesLength);

                                WriteOffset = 0;
                                ReadOffset += headerBytesLength;
                                numberOfBytesToProcess -= headerBytesLength;

                                MessageSize = BitConverter.ToInt32(HeaderBuffer, 0);

                                if (MessageSize <= 0 || MessageSize >= MaximumMessageSize)
                                {
                                    Connection.Dispose();
                                    numberOfBytesToProcess = 0;
                                }
                                BufferState = Enums.BufferState.Message;
                            }
                            else
                            {
                                Buffer.BlockCopy(bufferChunk, ReadOffset, HeaderBuffer, WriteOffset, numberOfBytesToProcess);
                                WriteOffset += numberOfBytesToProcess;
                                numberOfBytesToProcess = 0;
                            }
                            break;
                        case Enums.BufferState.Message:
                            if (MessageBuffer == null)
                            {
                                MessageBuffer = new byte[MessageSize];
                            }
                            else if (MessageBuffer.Length != MessageSize)
                            {
                                MessageBuffer = new byte[MessageSize];
                            }

                            int messageBytesLength = (numberOfBytesToProcess + WriteOffset > MessageSize) ? MessageSize - WriteOffset : numberOfBytesToProcess;

                            Buffer.BlockCopy(bufferChunk, ReadOffset, MessageBuffer, WriteOffset, messageBytesLength);

                            WriteOffset += messageBytesLength;
                            ReadOffset += messageBytesLength;
                            numberOfBytesToProcess -= messageBytesLength;

                            if (WriteOffset == MessageSize)
                            {
                                Connection.SetOnMessageReceived(MessageBuffer, MessageSize);
                                BufferState = Enums.BufferState.Message;
                                WriteOffset = 0;
                            }
                            break;
                    }
                    if (numberOfBytesToProcess == 0)
                    {
                        ReadOffset = 0;
                    }
                }
            }
        }
        #endregion
    }
}
