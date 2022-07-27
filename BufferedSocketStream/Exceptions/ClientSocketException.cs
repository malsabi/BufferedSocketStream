namespace BufferedSocketStream.Exceptions
{
    public class ClientSocketException : Exception
    {
        public DateTime Timestamp { get; set; }

        public ClientSocketException(string Message) : base(Message)
        {
            Timestamp = DateTime.Now;
        }

        public ClientSocketException(string Message, Exception InnerException) : base(Message, InnerException)
        {
            Timestamp = DateTime.Now;
        }
    }
}