using BufferedSocketStream.Common;

namespace BufferedSocketStream.Exceptions
{
    public class ServerListenerException : Exception
    {
        public IConnectionInfo Connection { get; set; }
        public DateTime Timestamp { get; set; }

        public ServerListenerException(string Message) : base(Message)
        {
            Timestamp = DateTime.Now;
        }

        public ServerListenerException(string Message, Exception InnerException) : base(Message, InnerException)
        {
            Timestamp = DateTime.Now;
        }
    }
}