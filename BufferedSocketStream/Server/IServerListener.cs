using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using BufferedSocketStream.Common;
using BufferedSocketStream.Exceptions;

namespace BufferedSocketStream.Server
{
    public interface IServerListener : IDisposable
    {
        #region "Properties"
        public bool IsListening { get; }

        public bool IsInitialized { get; }

        public ConcurrentDictionary<string, IConnectionInfo> Connections { get; }

        public ServerStatistics ServerStatistics { get; }

        public Socket SocketListener { get; }

        public SocketAsyncEventArgsPool ReadWritePool { get; }

        public ServerConfiguration Configuration { get; }
        #endregion

        #region "Events"
        public delegate void OnStartListenerDelegate(IServerListener sender, IPEndPoint endPoint);
        public event OnStartListenerDelegate OnStartListener;

        public delegate void OnStopListenerDelegate(IServerListener sender);
        public event OnStopListenerDelegate OnStopListener;

        public delegate void OnExceptionDelegate(IServerListener sender, ServerListenerException ex);
        public event OnExceptionDelegate OnException;

        public delegate void OnConnectionEstablishedDelegate(IServerListener sender, IConnectionInfo connection);
        public event OnConnectionEstablishedDelegate OnConnectionEstablished;

        public delegate void OnConnectionClosedDelegate(IServerListener sender, IConnectionInfo connection);
        public event OnConnectionClosedDelegate OnConnectionClosed;

        public delegate void OnConnectionMessageReceivedDelegate(IServerListener sender, IConnectionInfo connection, byte[] message, int messageLength);
        public event OnConnectionMessageReceivedDelegate OnConnectionMessageReceived;

        public delegate void OnConnectionMessageSentDelegate(IServerListener sender, IConnectionInfo connection, byte[] message, int messageLength);
        public event OnConnectionMessageSentDelegate OnConnectionMessageSent;

        public delegate void OnConnectionExceptionDelegate(IServerListener sender, ConnectionInfoException ex);
        public event OnConnectionExceptionDelegate OnConnectionException;
        #endregion

        #region "Public Methods"
        public void Initialize(ServerConfiguration configuration);

        public void StartListener();

        public void StopListener();

        public void StartAccept(SocketAsyncEventArgs args);

        public void BroadCast(byte[] message);
        #endregion
    }
}