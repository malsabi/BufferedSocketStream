using System.Net;
using System.Net.Sockets;
using BufferedSocketStream.Helpers;
using BufferedSocketStream.Exceptions;

namespace BufferedSocketStream.Common
{
    public class ConnectionInfo : IConnectionInfo
    {
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

        public event IConnectionInfo.OnExceptionDelegate OnException;
        #endregion

        #region "Public Methods"
        public ConnectionInfo(Socket Handle, Tuple<SocketAsyncEventArgs, SocketAsyncEventArgs> ConnectionAsyncEventArgs)
        {
            this.Handle = Handle;
            this.ConnectionAsyncEventArgs = ConnectionAsyncEventArgs;
        }

        public void Initialize(ConnectionConfiguration configuration)
        {
            try
            {
                if (ConnectionAsyncEventArgs == null || Handle == null)
                {
                    throw new ConnectionInfoException(this, "Failed to initialize the connection, one of the arguments passed was null.");
                }
                else
                {
                    Id = Guid.NewGuid();
                    EndPoint = (IPEndPoint)Handle.RemoteEndPoint;
                    ReceiveAsyncEventArgs = ConnectionAsyncEventArgs.Item1;
                    SendAsyncEventArgs = ConnectionAsyncEventArgs.Item2;
                    receiveDataManager = new ReceiveDataManager(configuration, this, ReceiveAsyncEventArgs);
                    sendDataManager = new SendDataManager(configuration, this, SendAsyncEventArgs);
                    Configuration = configuration;
                }
            }
            catch (ConnectionInfoException ex)
            {
                SetOnException(ex);
            }
        }

        public void StartReceiving()
        {
            try
            {
                if (IsClosed)
                {
                    throw new ConnectionInfoException(this, "Cannot start receiving messages since the connection is closed.");
                }
                else if (ConnectionAsyncEventArgs == null || Handle == null)
                {
                    throw new ConnectionInfoException(this, "Failed to start receiving messages, one of the arguments passed was null.");
                }
                else
                {
                    receiveDataManager.HandleReceiveAsync();
                }
            }
            catch (ConnectionInfoException ex)
            {
                SetOnException(ex);
            }
        }

        public void SendAsync(byte[] message)
        {
            try
            {
                if (IsClosed)
                {
                    throw new ConnectionInfoException(this, "Cannot send message since the connection is closed.");
                }
                else if (ConnectionAsyncEventArgs == null || Handle == null)
                {
                    throw new ConnectionInfoException(this, "Failed to send message, one of the arguments passed was null.");
                }
                else
                {
                    sendDataManager.HandleSendAsync(DataHelper.PrefixHeaderToMessage(message, Configuration.HeaderSize));
                }
            }
            catch (ConnectionInfoException ex)
            {
                SetOnException(ex);
            }
        }

        public void Dispose()
        {
            if (!IsClosed)
            {
                IsClosed = true;
                receiveDataManager.ClearBufferQueue();
                sendDataManager.ClearBufferQueue();
                try
                {
                    Handle.Shutdown(SocketShutdown.Both);
                }
                catch (ConnectionInfoException ex)
                {
                    SetOnException(ex);
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

        internal void SetOnException(ConnectionInfoException ex)
        {
            OnException?.Invoke(ex);
        }

        internal void SetOnClosed()
        {
            OnClosed?.Invoke(this);
        }
        #endregion
    }
}