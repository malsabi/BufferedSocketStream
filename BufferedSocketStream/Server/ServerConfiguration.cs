using System.Net;

namespace BufferedSocketStream.Server
{
    /// <summary>
    /// Used for configuring the server settings and the connection settings
    /// </summary>
    public class ServerConfiguration
    {
        #region "Properties"
        /// <summary>
        /// Specifies the local end point for the server to bind.
        /// </summary>
        public IPEndPoint EndPoint { get; set; }

        /// <summary>
        /// Represents the maximum back log pending connections.
        /// </summary>
        public int MaximumPendingConnections { get; set; }

        /// <summary>
        /// Sets the maximum connections the server can have.
        /// </summary>
        public int MaximumConnections { get; set; }

        /// <summary>
        /// Sets the maximum message the server can receive.
        /// </summary>
        public int MaximumMessageSize { get; set; }

        /// <summary>
        /// Sets the buffer size for the received data to be stored in.
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// Sets the header size that will be prefixed with the message,
        /// usually contains information about the message such as length.
        /// </summary>
        public int HeaderSize { get; set; }

        /// <summary>
        /// Sets the path for the logs file which is in sqlite format.
        /// </summary>
        public string LogsPath { get; set; }

        /// <summary>
        /// Represents the amount of time to wait for the data to be sent and then
        /// closes socket connection and releases all associated resources.
        /// </summary>
        public int ShutdownTimeout { get; set; }
        #endregion

        /// <summary>
        /// Uses the default configuration for the following properties:
        /// <para><see cref="EndPoint"/> Sets the IP to 127.0.0.1 and the Port to 1669.</para>
        /// <para><see cref="MaximumPendingConnections"/> is set to 100.</para>
        /// <para><see cref="MaximumConnections"/> is set to 500.</para>
        /// <para><see cref="MaximumMessageSize"/> is set to 1MB.</para>
        /// <para><see cref="BufferSize"/> is set to 16KB.</para>
        /// <para><see cref="HeaderSize"/> is set to 4 bytes.</para>
        /// <para><see cref="LogsPath"/> is set to the current execution directory (./Logs.sqlite).</para>
        /// </summary>
        public ServerConfiguration()
        {
            EndPoint = new IPEndPoint(IPAddress.Loopback, 1669);
            MaximumPendingConnections = 100;
            MaximumConnections = 500;
            MaximumMessageSize = 1024 * 1024;
            BufferSize = 1024 * 16;
            HeaderSize = 4;
            LogsPath = "./Logs.sqlite";
            ShutdownTimeout = 5000;
        }
    }
}