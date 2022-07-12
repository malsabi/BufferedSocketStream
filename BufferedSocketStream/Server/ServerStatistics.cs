namespace BufferedSocketStream.Server
{
    /// <summary>
    /// Represents a statistical information of the server performance.
    /// </summary>
    public class ServerStatistics
    {
        /// <summary>
        /// Represents the total saved bytes received from every running instance
        /// of the server.
        /// </summary>
        public long TotalBytesReceived { get; set; }

        /// <summary>
        /// Represents the total saved bytes sent from every running instance
        /// of the server.
        /// </summary>
        public long TotalBytesSent { get; set; }

        /// <summary>
        /// Represents the total established connections from ever running
        /// instance of the server.
        /// </summary>
        public long TotalEstablishedConnections { get; set; }

        /// <summary>
        /// Represents the total number of blocked connections from every
        /// running instance of the server.
        /// </summary>
        public long TotalBlockedConnections { get; set; }

        /// <summary>
        /// Represents the total bytes received from the current running
        /// instance of the server.
        /// </summary>
        public long CurrentTotalBytesReceived { get; set; }

        /// <summary>
        /// Represents the total bytes sent from the current running
        /// instance of the server.
        /// </summary>
        public long CurrentTotalBytesSent { get; set; }

        /// <summary>
        /// Represents the total blocked connections from the current
        /// running instance of the server.
        /// </summary>
        public long CurrentBlockedConnections { get; set; }

        /// <summary>
        /// Represents the total established connections from the current
        /// runing instance of the server.
        /// </summary>
        public long CurrentEstablishedConnections { get; set; }
    }
}