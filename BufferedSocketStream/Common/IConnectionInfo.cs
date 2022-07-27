using System.Net;
using System.Net.Sockets;
using BufferedSocketStream.Exceptions;

namespace BufferedSocketStream.Common
{
    public interface IConnectionInfo : IDisposable
    {
        #region "Properties"
        public bool IsClosed { get; }

        public Socket Handle { get; }

        public IPEndPoint EndPoint { get; }

        public Tuple<SocketAsyncEventArgs, SocketAsyncEventArgs> ConnectionAsyncEventArgs { get; }

        public SocketAsyncEventArgs ReceiveAsyncEventArgs { get; }

        public SocketAsyncEventArgs SendAsyncEventArgs { get; }

        public ConnectionConfiguration Configuration { get; }

        public Guid Id { get; }
        #endregion

        #region "Events"
        public delegate void OnMessageReceivedDelegate(IConnectionInfo connection, byte[] message, int messageLength);
        public event OnMessageReceivedDelegate OnMessageReceived;

        public delegate void OnMessageSentDelegate(IConnectionInfo connection, byte[] message, int messageLength);
        public event OnMessageSentDelegate OnMessageSent;

        public delegate void OnExceptionDelegate(ConnectionInfoException ex);
        public event OnExceptionDelegate OnException;

        public delegate void OnClosedDelegate(IConnectionInfo connection);
        public event OnClosedDelegate OnClosed;
        #endregion

        #region "Methods"
        public void Initialize(ConnectionConfiguration configuration);

        public void SendAsync(byte[] message);

        public void StartReceiving();
        #endregion
    }
}