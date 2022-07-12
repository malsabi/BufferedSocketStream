using BufferedSocketStream.Common;
using BufferedSocketStream.Server;
using System.Net;

ServerListener serverListener = new ServerListener();
serverListener.OnStartListener += ServerListener_OnStartListener;
serverListener.OnStopListener += ServerListener_OnStopListener;
serverListener.OnException += ServerListener_OnException;
serverListener.OnConnectionEstablished += ServerListener_OnConnectionEstablished;
serverListener.OnConnectionMessageReceived += ServerListener_OnConnectionMessageReceived;
serverListener.OnConnectionMessageSent += ServerListener_OnConnectionMessageSent;
serverListener.OnConnectionClosed += ServerListener_OnConnectionClosed;
serverListener.OnConnectionException += ServerListener_OnConnectionException;
serverListener.Initialize(new ServerConfiguration());
serverListener.StartListener();
serverListener.StartAccept(null);
Console.ReadKey();

void ServerListener_OnStartListener(IServerListener sender, IPEndPoint endPoint)
{
    Console.WriteLine("Server started listening on: {0}", endPoint);
}

void ServerListener_OnStopListener(IServerListener sender)
{
    Console.WriteLine("Server stopped listening on: " + sender.Configuration.EndPoint);
}

void ServerListener_OnException(IServerListener sender, Exception ex)
{
    Console.WriteLine("Server exception: {0}", ex.Message);
}

void ServerListener_OnConnectionEstablished(IServerListener sender, IConnectionInfo connection)
{
    Console.WriteLine("Connection[{0}] established successfully", connection.EndPoint);
}

void ServerListener_OnConnectionMessageReceived(IServerListener sender, IConnectionInfo connection, byte[] message, int messageLength)
{
    Console.WriteLine("Connection[{0}] received: {1}", connection.EndPoint, messageLength);
}

void ServerListener_OnConnectionMessageSent(IServerListener sender, IConnectionInfo connection, byte[] message, int messageLength)
{
    Console.WriteLine("Connection[{0}] sent: {1}", connection.EndPoint, messageLength);
}

void ServerListener_OnConnectionClosed(IServerListener sender, IConnectionInfo connection)
{
    Console.WriteLine("Connection[{0}] closed", connection.EndPoint);
}

void ServerListener_OnConnectionException(IServerListener sender, IConnectionInfo connection, Exception ex)
{
    Console.WriteLine("Connection[{0}] exception: {1}", connection.EndPoint, ex.Message);
}
