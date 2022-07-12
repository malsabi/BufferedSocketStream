using System.Net;
using System.Net.Sockets;
using BufferedSocketStream.Common;

namespace BufferedSocketStream.Client
{
    public class ClientSocket : ConnectionInfo, IClientSocket
    {
        public ClientSocket() : base(null, null)
        {
        }

    }
}