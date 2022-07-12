using System.Net;
using System.Net.Sockets;
using BufferedSocketStream.Helpers;
using BufferedSocketStream.Common.DataManagement;

namespace BufferedSocketStream.Common
{
    public class ConnectionInfo : IConnectionInfo
    {
        #region "Fields"
        private SendDataManager sendDataManager;
        private ReceiveDataManager receiveDataManager;
        #endregion

        #region "Properties"
        public bool IsClosed { get; private set; }

        public Socket Handle { get; private set; }

        public IPEndPoint EndPoint { get; private set; }

        public Tuple<SocketAsyncEventArgs, SocketAsyncEventArgs> ConnectionAsyncEventArgs { get; private set; }

        public SocketAsyncEventArgs ReceiveAsyncEventArgs { get; private set; }

        public SocketAsyncEventArgs SendAsyncEventArgs { get; private set; }

        public Guid Id { get; private set; }

        public ConnectionConfiguration Configuration { get; private set; }
        #endregion

        #region "Events"
        public event IConnectionInfo.OnMessageReceivedDelegate OnMessageReceived;

        public event IConnectionInfo.OnMessageSentDelegate OnMessageSent;

        public event IConnectionInfo.OnClosedDelegate OnClosed;
        #endregion

        #region "Public Methods"
        public ConnectionInfo(Socket Handle, Tuple<SocketAsyncEventArgs, SocketAsyncEventArgs> ConnectionAsyncEventArgs)
        {
            this.Handle = Handle;
            this.ConnectionAsyncEventArgs = ConnectionAsyncEventArgs;
        }

        public void Initialize(ConnectionConfiguration configuration)
        {
            if (ConnectionAsyncEventArgs == null || Handle == null)
            {
                return;
            }
            IsClosed = false;
            Id = Guid.NewGuid();
            EndPoint = (IPEndPoint)Handle.RemoteEndPoint;
            ReceiveAsyncEventArgs = ConnectionAsyncEventArgs.Item1;
            ReceiveAsyncEventArgs.SetBuffer(new byte[configuration.BufferSize], 0, configuration.BufferSize);
            SendAsyncEventArgs = ConnectionAsyncEventArgs.Item2;
            SendAsyncEventArgs.SetBuffer(new byte[configuration.BufferSize], 0, configuration.BufferSize);
            receiveDataManager = new ReceiveDataManager(configuration, this, ReceiveAsyncEventArgs);
            sendDataManager = new SendDataManager(configuration, this, SendAsyncEventArgs);
            Configuration = configuration;
        }

        public void StartReceiving()
        {
            if (IsClosed == true || Handle == null || ReceiveAsyncEventArgs == null)
            {
                return;
            }
            receiveDataManager.HandleReceiveAsync();
        }

        public void SendAsync(byte[] message)
        {
            if (IsClosed == true || Handle == null || SendAsyncEventArgs == null)
            {
                return;
            }
            sendDataManager.HandleSendAsync(DataHelper.PrefixHeaderToMessage(message, Configuration.HeaderSize));
        }

        public void Dispose()
        {
            if (IsClosed == false)
            {
                IsClosed = true;
                receiveDataManager.ClearBufferQueue();
                sendDataManager.ClearBufferQueue();
                try
                {
                    Handle.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    Handle.Close();
                }
                SetOnClosed();
            }
            GC.SuppressFinalize(this);
        }
        #endregion

        #region "Handlers"
        internal void SetOnMessageReceived(byte[] message, int messageLength)
        {
            OnMessageReceived?.Invoke(this, message, messageLength);
        }

        internal void SetOnMessageSent(byte[] message, int messageLength)
        {
            OnMessageSent?.Invoke(this, message, messageLength);
        }

        internal void SetOnClosed()
        {
            OnClosed?.Invoke(this);
        }
        #endregion
    }
}