using System;
using System.Threading;
using System.Collections.Generic;

namespace BufferedSocketStream.BufferManager
{
    internal class Program
    {
        private static int MaximumConnections = 1000;
        private static int MaximumIO = 2;
        private static int BufferSize = 1024 * 100; //100KB
        private static BufferManager BM;
        private static List<Client> Clients = new List<Client>();

        static void Main(string[] args)
        {
            InitializeBufferManager();
            Thread.Sleep(3000);
            FillClientsWithBuffers();
            Thread.Sleep(3000);
            ClearClientsAndReturnBuffers();
            Thread.Sleep(3000);
            ClearBufferManager();
            Console.ReadKey();
        }

        private static void InitializeBufferManager()
        {
            BM = new BufferManager(MaximumConnections * MaximumIO, BufferSize);
            Console.WriteLine("Finsihed Initalizing BufferManager");
        }

        private static void FillClientsWithBuffers()
        {
            for (int i = 0; i < MaximumConnections; i++)
            {
                Client client = new Client(BM.GetBuffer(), BM.GetBuffer());
                client.FillReceiveBuffer();
                client.FillSendBuffer();
                client.PrintContentOfReceiveBuffer();
                client.PrintContentOfSendBuffer();
                Clients.Add(client);
            }
            Console.WriteLine("Finished adding {0} clients", MaximumConnections);
        }

        private static void ClearClientsAndReturnBuffers()
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                BM.SetBuffer(Clients[i].ReceiveBuffer, false);
                BM.SetBuffer(Clients[i].SendBuffer, false);
                Clients[i].Dispose();
                Clients.RemoveAt(i);
            }
            Clients = null;
            Console.WriteLine("Finished clearing the clients list and returned the buffers to the buffer manager");
        }

        private static void ClearBufferManager()
        {
            BM.Clear();
            GC.Collect();
            Console.Write("Successfully cleared the buffer manager");
        }
    }
}