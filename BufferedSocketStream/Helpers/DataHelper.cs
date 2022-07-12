namespace BufferedSocketStream.Helpers
{
    public static class DataHelper
    {
        public static byte[] PrefixHeaderToMessage(byte[] message, int headerSize)
        {
            if (headerSize <= 0)
            {
                throw new Exception("Invalid header size, cannot be less or equal to zero");
            }
            byte[] packet = new byte[message.Length + headerSize];
            Buffer.BlockCopy(BitConverter.GetBytes(message.Length), 0, packet, 0, headerSize);
            Buffer.BlockCopy(message, 0, packet, headerSize, message.Length);
            return packet;
        }
    }
}