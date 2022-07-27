using BufferedSocketStream.Client;
using BufferedSocketStream.Exceptions;

ClientSocket client = new ClientSocket();
client.OnConnected += Client_OnConnected;
client.OnMessageReceived += Client_OnMessageReceived;
client.OnMessageSent += Client_OnMessageSent;
client.OnException += Client_OnException;
client.OnClosed += Client_OnClosed;

client.Initialize(new ClientConfiguration());
client.Connect();
client.StartReceiving();
Console.ReadKey();

void Client_OnConnected(IClientSocket client)
{
    Console.WriteLine("Client Connected: {0}", client.EndPoint);
    byte[] msg = new byte[1024 * 1024];
    for (int i = 0; i < 1200; i++)
    {
        client.SendAsync(msg);
    }
}

void Client_OnMessageReceived(IClientSocket client, byte[] message, int messageLength)
{
    Console.WriteLine("Client received a message of size: {0}", messageLength); 
}

void Client_OnMessageSent(IClientSocket client, byte[] message, int messageLength)
{
    Console.WriteLine("Client sent a message of size: {0}", messageLength);
}

void Client_OnException(ClientSocketException ex)
{
    Console.WriteLine("Client Exception: {0}", ex.Message);
}

void Client_OnClosed(IClientSocket client)
{
    Console.WriteLine("Client Closed: {0}", client.EndPoint);
}