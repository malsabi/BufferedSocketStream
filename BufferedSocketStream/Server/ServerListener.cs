using System.Net.Sockets;
using System.Collections.Concurrent;
using BufferedSocketStream.Common;
using BufferedSocketStream.Exceptions;

namespace BufferedSocketStream.Server
{
    public class ServerListener : IServerListener
    {
        #region "Properties"
        public bool IsListening { get; private set; }

        public bool IsInitialized { get; private set; }

        public ConcurrentDictionary<string, IConnectionInfo> Connections { get; private set; }

        public ServerStatistics ServerStatistics { get; private set; }

        public Socket SocketListener { get; private set; }

        public SocketAsyncEventArgsPool ReadWritePool { get; private set; }

        public ServerConfiguration Configuration { get; private set; }

        public BufferManager Buffer { get; private set; }
        #endregion

        #region "Events"
        public event IServerListener.OnStartListenerDelegate OnStartListener;

        public event IServerListener.OnStopListenerDelegate OnStopListener;

        public event IServerListener.OnExceptionDelegate OnException;

        public event IServerListener.OnConnectionEstablishedDelegate OnConnectionEstablished;

        public event IServerListener.OnConnectionClosedDelegate OnConnectionClosed;

        public event IServerListener.OnConnectionMessageReceivedDelegate OnConnectionMessageReceived;

        public event IServerListener.OnConnectionMessageSentDelegate OnConnectionMessageSent;

        public event IServerListener.OnConnectionExceptionDelegate OnConnectionException;
        #endregion

        #region "Public Methods"
        public ServerListener()
        {
            Connections = null!;
            ServerStatistics = null!;
            SocketListener = null!;
            ReadWritePool = null!;
            Configuration = null!;
        }

        public void Initialize(ServerConfiguration configuration)
        {
            try
            {
                if (IsInitialized)
                {
                    throw new ServerListenerException("Failed to initialize since the server is already initialized.");
                }
                else
                {
                    Connections = new ConcurrentDictionary<string, IConnectionInfo>();
                    ServerStatistics = new ServerStatistics();
                    SocketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        LingerState = new LingerOption(true, configuration.ShutdownTimeout)
                    };
                    ReadWritePool = new SocketAsyncEventArgsPool(configuration.MaximumConnections);
                    for (int i = 0; i < configuration.MaximumConnections; i++)
                    {
                        SocketAsyncEventArgs receiveAsyncEventArgs = new();
                        receiveAsyncEventArgs.SetBuffer(new byte[configuration.BufferSize], 0, configuration.BufferSize);
                        SocketAsyncEventArgs sendAsyncEventArgs = new();
                        sendAsyncEventArgs.SetBuffer(new byte[configuration.BufferSize], 0, configuration.BufferSize);
                        ReadWritePool.Push(new Tuple<SocketAsyncEventArgs, SocketAsyncEventArgs>(receiveAsyncEventArgs, sendAsyncEventArgs));
                    }
                    Configuration = configuration;
                    IsInitialized = true;
                }
            }
            catch (ServerListenerException ex)
            {
                SetOnException(ex);
            }
        }

        public void StartListener()
        {
            try
            {
                if (!IsInitialized)
                {
                    throw new ServerListenerException("Failed on starting server listener since the server is not initialized.");
                }
                else if (IsListening)
                {
                    throw new ServerListenerException("Failed on starting server listener since the server is already listening.");
                }
                else
                {
                    SocketListener.Bind(Configuration.EndPoint);
                    SocketListener.Listen(Configuration.MaximumPendingConnections);
                    SetOnStartListener();
                    IsListening = true;
                }
            }
            catch (ServerListenerException ex)
            {
                SetOnException(ex);
            }
        }

        public void StopListener()
        {
            try
            {
                if (!IsInitialized)
                {
                    throw new ServerListenerException("Failed on stopping server listener since the server is not initialized.");
                }
                else if (!IsListening)
                {
                    throw new ServerListenerException("Failed on stopping server listener since the server is already not listening.");
                }
                else
                {
                    SocketListener.Shutdown(SocketShutdown.Both);
                }
            }
            catch (ServerListenerException ex)
            {
                SetOnException(ex);
            }
            finally
            {
                SocketListener.Close(Configuration.ShutdownTimeout);
                IsInitialized = false;
                IsListening = false;
                SetOnStopListener();
            }
        }

        public void StartAccept(SocketAsyncEventArgs acceptAsyncEventArgs)
        {
            try
            {
                if (!IsInitialized)
                {
                    throw new ServerListenerException("Cannot start accepting connections since the server is not initialized.");
                }
                else if (!IsListening)
                {
                    throw new ServerListenerException("Cannot start accepting connections since the server is not listening.");
                }
                else
                {
                    if (acceptAsyncEventArgs == null)
                    {
                        acceptAsyncEventArgs = new SocketAsyncEventArgs();
                        acceptAsyncEventArgs.Completed += OnAcceptAsyncCompleted;
                    }
                    else
                    {
                        acceptAsyncEventArgs.AcceptSocket = null;
                    }
                    if (!SocketListener.AcceptAsync(acceptAsyncEventArgs))
                    {
                        ProcessAccept(acceptAsyncEventArgs);
                    }
                }
            }
            catch (ServerListenerException ex)
            {
                SetOnException(ex);
            }
        }

        public void BroadCast(byte[] message)
        {
            try
            {
                if (Connections == null)
                {
                    throw new ServerListenerException("Failed on broad casting the message since their is no active connections available.");
                }
                else
                {
                    foreach (KeyValuePair<string, IConnectionInfo> pair in Connections)
                    {
                        pair.Value.SendAsync(message);
                    }
                }
            }
            catch (ServerListenerException ex)
            {
                SetOnException(ex);
            }
        }

        public IConnectionInfo GetClient(string Id)
        {
            try
            {
                if (Connections == null || Connections.IsEmpty)
                {
                    throw new ServerListenerException("Failed on getting the client since their is no active connections available.");
                }
                if (!Connections.TryGetValue(Id, out IConnectionInfo result))
                {
                    throw new ServerListenerException(string.Format("Could not find a client with the following id[{0}].", Id));
                }
                return result;
            }
            catch (ServerListenerException ex)
            {
                SetOnException(ex);
                return default;
            }
        }

        public void AddConnectionToBlacklist(IConnectionInfo connection)
        {

        }

        public void RemoveConnectionFromBlacklist(IConnectionInfo connection)
        {
        }

        public void Dispose()
        {
            StopListener();
            Connections.Clear();
            ReadWritePool.ClearPool();
            IsListening = false;
            IsInitialized = false;
            GC.SuppressFinalize(this);
        }
        #endregion

        #region "Handlers"
        private void SetOnStartListener()
        {
            OnStartListener?.Invoke(this, Configuration.EndPoint);
        }

        private void SetOnStopListener()
        {
            OnStopListener?.Invoke(this);
        }

        private void SetOnException(ServerListenerException ex)
        {
            OnException?.Invoke(this, ex);
        }

        private void SetOnConnectionEstablished(IConnectionInfo connection)
        {
            OnConnectionEstablished?.Invoke(this, connection);
        }

        private void SetOnConnectionClosed(IConnectionInfo connection)
        {
            try
            {
                if (!HandleClosedConnection(connection))
                {
                   throw new ServerListenerException("Failed to handle closing the connection.");
                }
                OnConnectionClosed?.Invoke(this, connection);
            }
            catch (ServerListenerException ex)
            {
                SetOnException(ex);
            }
        }

        private void SetOnConnectionMessageReceived(IConnectionInfo connection, byte[] message, int messageLength)
        {
            OnConnectionMessageReceived?.Invoke(this, connection, message, messageLength);
            ServerStatistics.CurrentTotalBytesReceived += messageLength;
        }

        private void SetOnConnectionMessageSent(IConnectionInfo connection, byte[] message, int messageLength)
        {
            OnConnectionMessageSent?.Invoke(this, connection, message, messageLength);
            ServerStatistics.CurrentTotalBytesSent += messageLength;
        }

        private void SetOnConnectionException(ConnectionInfoException ex)
        {
            OnConnectionException?.Invoke(this, ex);
        }

        private void OnAcceptAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Accept:
                        ProcessAccept(e);
                        break;
                    default:
                        throw new ServerListenerException("The last operation completed on the socket was not an accept operation.");
                }
            }
            catch (ServerListenerException ex)
            {
                SetOnException(ex);
            }
        }
        #endregion

        #region "Private Methods"
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError != SocketError.Success)
                {
                    throw new ServerListenerException("Failed to process the acception of the new connection.");
                }
                Socket handle = e.AcceptSocket;
                if (handle == null)
                {
                    throw new ServerListenerException("Could not get a socket instance from the accepted connection.");
                }
                Tuple<SocketAsyncEventArgs, SocketAsyncEventArgs> item = ReadWritePool.Pop();
                if (item == null)
                {
                    throw new ServerListenerException("Could not retrieve a socket operation from the pool.");
                }
                IConnectionInfo connection = new ConnectionInfo(handle, item);
                if (!HandleNewConnection(connection))
                {
                    throw new ConnectionInfoException(connection, "Failed to add the new connection into the connections list.");
                }
                else
                {
                    SetOnConnectionEstablished(connection);
                }
            }
            catch (ConnectionInfoException ex)
            {
                SetOnConnectionException(ex);
            }
            catch (ServerListenerException ex)
            {
                SetOnException(ex);
            }
            StartAccept(e);
        }

        private bool HandleNewConnection(IConnectionInfo connection)
        {
            connection.Initialize(new ConnectionConfiguration(Configuration));
            if (Connections.TryAdd(connection.Id.ToString(), connection))
            {
                connection.OnMessageReceived += SetOnConnectionMessageReceived;
                connection.OnMessageSent += SetOnConnectionMessageSent;
                connection.OnException += SetOnConnectionException;
                connection.OnClosed += SetOnConnectionClosed;
                connection.StartReceiving();
                ServerStatistics.CurrentEstablishedConnections += 1;
                return true;
            }
            return false;
        }

        private bool HandleClosedConnection(IConnectionInfo connection)
        {
            if (Connections.TryRemove(connection.Id.ToString(), out IConnectionInfo connectionToRemove))
            {
                connectionToRemove.OnMessageReceived -= SetOnConnectionMessageReceived;
                connectionToRemove.OnMessageSent -= SetOnConnectionMessageSent;
                connection.OnException -= SetOnConnectionException;
                connectionToRemove.OnClosed -= SetOnConnectionClosed;
                if (connectionToRemove.ConnectionAsyncEventArgs != null)
                {
                    ReadWritePool.Push(connectionToRemove.ConnectionAsyncEventArgs);
                }
                ServerStatistics.CurrentEstablishedConnections -= 1;
                return true;
            }
            return false;
        }
        #endregion
    }
}