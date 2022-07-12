using BufferedSocketStream.Server;

namespace BufferedSocketStream.Common
{
    public class ConnectionConfiguration
    {
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


        public ConnectionConfiguration(ServerConfiguration configuation)
        {
            MaximumMessageSize = configuation.MaximumMessageSize;
            BufferSize = configuation.BufferSize;
            HeaderSize = configuation.HeaderSize;
        }

        //public ConnectionConfiguration(ClientConfiguration configuration)
        //{
        //}
    }
}