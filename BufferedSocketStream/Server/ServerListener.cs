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
            if (IsInitialized)
            {
                return;
            }
            try
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
            catch (ServerListenerException ex)
            {
                SetOnException(ex);
                IsInitialized = false;
            }
        }

        public void StartListener()
        {
            try
            {
                if (IsInitialized == false)
                {
                    throw new ServerListenerException("Cannot start listening since the server is not initialized.");
                }
                if (IsListening)
                {
                    throw new ServerListenerException("Cannot start listening since the server is already listening.");
                }
                SocketListener.Bind(Configuration.EndPoint);
                SocketListener.Listen(Configuration.MaximumPendingConnections);
                SetOnStartListener();
                IsListening = true;
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
                if (IsInitialized == false)
                {
                    throw new ServerListenerException("Failed on stopping server listener since the server is not initialized.");
                }
                if (IsListening == false)
                {
                    throw new ServerListenerException("Failed on stopping server listener since the server is already not listening.");
                }
                SocketListener.Shutdown(SocketShutdown.Both);
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
                if (IsInitialized == false)
                {
                    throw new ServerListenerException("Cannot start accepting connections since the server is not initialized.");
                }
                if (IsListening == false)
                {
                    throw new ServerListenerException("Cannot start accepting connections since the server is not listening.");
                }
                if (acceptAsyncEventArgs == null)
                {
                    acceptAsyncEventArgs = new SocketAsyncEventArgs();
                    acceptAsyncEventArgs.Completed += OnAcceptAsyncCompleted;
                }
                else
                {
                    acceptAsyncEventArgs.AcceptSocket = null;
                }
                if (SocketListener.AcceptAsync(acceptAsyncEventArgs) == false)
                {
                    ProcessAccept(acceptAsyncEventArgs);
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
                    throw new ServerListenerException("Cannot broad cast the message since their is no active connections available.");
                }
                foreach (KeyValuePair<string, IConnectionInfo> pair in Connections)
                {
                    pair.Value.SendAsync(message);
                }
            }
            catch (ServerListenerException ex)
            {
                SetOnException(ex);
            }
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
                if (HandleClosedConnection(connection) == false)
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
        }

        private void SetOnConnectionMessageSent(IConnectionInfo connection, byte[] message, int messageLength)
        {
            OnConnectionMessageSent?.Invoke(this, connection, message, messageLength);
        }

        private void SetOnConnectionException(ConnectionInfoException ex)
        {
            OnConnectionException?.Invoke(this, ex);
        }

        private void OnAcceptAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    ProcessAccept(e);
                    break;
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
                    throw new ServerListenerException("Could not get a socket instance from the incoming connection.");
                }
                Tuple<SocketAsyncEventArgs, SocketAsyncEventArgs> item = ReadWritePool.Pop();
                if (item == null)
                {
                    throw new ServerListenerException("Could not retrieve a socket operation from the pool.");
                }
                IConnectionInfo connection = new ConnectionInfo(handle, item);
                if (HandleNewConnection(connection) == false)
                {
                    throw new ConnectionInfoException(connection, "Failed to add the new connection into the connections list.");
                }
                SetOnConnectionEstablished(connection);
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
                connection.OnClosed += SetOnConnectionClosed;
                connection.StartReceiving();
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
                connectionToRemove.OnClosed -= SetOnConnectionClosed;
                if (connectionToRemove.ConnectionAsyncEventArgs != null)
                {
                    ReadWritePool.Push(connectionToRemove.ConnectionAsyncEventArgs);
                }
                return true;
            }
            return false;
        }
        #endregion
    }
}