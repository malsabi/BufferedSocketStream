using BufferedSocketStream.Common;

namespace BufferedSocketStream.Exceptions
{
    public class ConnectionInfoException : Exception
    {
        public IConnectionInfo Connection { get; set; }
        public DateTime Timestamp { get; set; }

        public ConnectionInfoException(string Message) : base(Message)
        {
            Timestamp = DateTime.Now;
        }

        public ConnectionInfoException(string Message, Exception InnerException) : base(Message, InnerException)
        {
            Timestamp = DateTime.Now;
        }

        public ConnectionInfoException(IConnectionInfo Connection, string Message) : base(Message)
        {
            this.Connection = Connection;
            Timestamp = DateTime.Now;
        }

        public ConnectionInfoException(IConnectionInfo Connection, string Message, Exception InnerException) : base(Message, InnerException)
        {
            this.Connection = Connection;
            Timestamp = DateTime.Now;
        }
    }
}