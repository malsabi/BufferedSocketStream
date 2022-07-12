using System.Net.Sockets;

namespace BufferedSocketStream.Common.DataManagement
{
    internal class SendDataManager : DataManagerBase
    {
        #region "Properties"
        public ConnectionConfiguration Configuration { get; private set; }
        public ConnectionInfo Connection { get; private set; }
        public SocketAsyncEventArgs SendAsyncEventArgs { get; private set; }
        #endregion

        public SendDataManager(ConnectionConfiguration Configuration, ConnectionInfo Connection, SocketAsyncEventArgs SendAsyncEventArgs) : base()
        {
            this.Configuration = Configuration;
            this.Connection = Connection;
            this.SendAsyncEventArgs = SendAsyncEventArgs;
            Initialize();
        }

        #region "Internal Methods"
        internal void HandleSendAsync(byte[] message)
        {
            if (Connection != null && Connection.Handle != null && Connection.IsClosed == false)
            {
                return;
            }
            if (BufferQueue == null)
            {
                return;
            }

            if (IsBuffering)
            {
                BufferQueue.Enqueue(message);
            }
            else
            {
                MessageSize = message.Length;
                if (MessageSize >= BufferSize)
                {
                    SendAsyncEventArgs.SetBuffer(0, BufferSize);
                    Buffer.BlockCopy(message, 0, SendAsyncEventArgs.Buffer, 0, BufferSize);
                    ReadOffset = MessageSize - BufferSize;
                    WriteOffset = BufferSize;
                }
                else
                {
                    SendAsyncEventArgs.SetBuffer(0, MessageSize);
                    Buffer.BlockCopy(message, 0, SendAsyncEventArgs.Buffer, 0, MessageSize);
                    ReadOffset = 0;
                    WriteOffset = MessageSize;
                }
                IsBuffering = true;
                if (Connection.Handle.SendAsync(SendAsyncEventArgs) == false)
                {
                    ProcessSendAsync(SendAsyncEventArgs);
                }
            }
        }

        internal void ClearBufferQueue()
        {
            Dispose();
        }
        #endregion

        #region "Private Methods"
        private void Initialize()
        {
            SendAsyncEventArgs.Completed += OnSendAsyncCompleted;
            MaximumMessageSize = Configuration.MaximumMessageSize;
            HeaderSize = Configuration.HeaderSize;
            BufferSize = Configuration.BufferSize;
        }

        private void ProcessSendAsync(SocketAsyncEventArgs e)
        {
            if (Connection != null && Connection.IsClosed == false)
            {
                return;
            }

            if (e.SocketError == SocketError.Success)
            {
                //If there was no bytes transfered then dispose the connection.
                if (e.BytesTransferred == 0)
                {
                    Connection.Dispose();
                    return;
                }

                //The message was sent and it was below the buffer size.
                if (ReadOffset == 0)
                {
                    //Reset WriteOffset
                    WriteOffset = 0;

                    //Reset IsBuffering, to continue sending messages
                    IsBuffering = false;

                    //If the buffer queue is not empty then there is pending messages.
                    if (BufferQueue != null && BufferQueue.IsEmpty == false)
                    {
                        //Extract the message from the buffer queue.
                        BufferQueue.TryDequeue(out byte[] nextMessage);
                        //Send the message again by calling 'HandleSendAsync' again.
                        HandleSendAsync(nextMessage);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    //Get the accurate count to process the remaning data from the message.
                    int count = ReadOffset <= BufferSize ? ReadOffset : BufferSize;
                    //Set the buffer from start till the count.
                    SendAsyncEventArgs.SetBuffer(0, count);
                    //Copy the remaining data and store them into the buffer by count amount.
                    Buffer.BlockCopy(Message, WriteOffset, SendAsyncEventArgs.Buffer, 0, count);
                    //Adjust offsets
                    ReadOffset -= count;
                    WriteOffset += count;
                    //Send the remaining data and process it.
                    if (Connection.Handle.SendAsync(SendAsyncEventArgs) == false)
                    {
                        ProcessSendAsync(SendAsyncEventArgs);
                    }
                }
            }
            else
            {
                Connection.Dispose();
            }
        }

        private void OnSendAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    ProcessSendAsync(e);
                    break;
                default:
                    Connection.Dispose();
                    break;
            }
        }
        #endregion
    }
}