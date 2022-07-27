using System;

namespace BufferedSocketStream.BufferManager
{
    public class Client : IDisposable
    {
        private BufferManager.BufferObject receiveBuffer;
        public BufferManager.BufferObject ReceiveBuffer
        {
            get
            {
                return receiveBuffer;
            }
            set
            {
                receiveBuffer = value;
            }
        }

        private BufferManager.BufferObject sendBuffer;
        public BufferManager.BufferObject SendBuffer
        {
            get
            {
                return sendBuffer;
            }
            set
            {
                sendBuffer = value;
            }
        }

        public Client(BufferManager.BufferObject ReceiveBuffer, BufferManager.BufferObject SendBuffer)
        {
            this.ReceiveBuffer = ReceiveBuffer;
            this.SendBuffer = SendBuffer;
        }

        public void FillReceiveBuffer()
        {
            string randomText = "This is from FillReceiveBuffer";
            byte[] randomTextBytes = System.Text.Encoding.ASCII.GetBytes(randomText);
            ReceiveBuffer.FillWith(randomTextBytes, 0, randomTextBytes.Length);
        }

        public void PrintContentOfReceiveBuffer()
        {
            string randomText = System.Text.Encoding.ASCII.GetString(ReceiveBuffer.Bytes, 0, (int)ReceiveBuffer.TotalWriteBytes);
            Console.WriteLine(randomText);
        }

        public void FillSendBuffer()
        {
            string randomText = "This is from FillSendBuffer";
            byte[] radnomTextBytes = System.Text.Encoding.ASCII.GetBytes(randomText);
            SendBuffer.FillWith(radnomTextBytes, 0, radnomTextBytes.Length);
        }

        public void PrintContentOfSendBuffer()
        {
            string randomText = System.Text.Encoding.ASCII.GetString(SendBuffer.Bytes, 0, (int)SendBuffer.TotalWriteBytes);
            Console.WriteLine(randomText);
        }

        public void Dispose()
        {
            receiveBuffer = null;
            sendBuffer = null;
            GC.SuppressFinalize(this);
        }
    }
}