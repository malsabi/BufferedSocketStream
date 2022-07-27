using System;

namespace BufferedSocketStream.BufferManager
{
    public class BufferObject : IDisposable
    {
        #region "Properties"
        public byte[] Bytes { get; private set; }
        public bool IsDisposed { get; private set; }
        public long TotalWriteBytes { get; private set; }
        public long TotalReadBytes { get; private set; }
        #endregion

        public BufferObject(int bufferSize)
        {
            Bytes = new byte[bufferSize];
            IsDisposed = false;
        }

        #region "Public Methods"
        public int GetLength()
        {
            return Bytes == null ? 0 : Bytes.Length;
        }

        public void FillWith(byte[] sourceArray, long sourceIndex, long length)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BufferObject));
            }

            if (sourceArray == null)
            {
                throw new ArgumentNullException(nameof(sourceArray));
            }

            if (GetLength() == 0)
            {
                throw new ArgumentException("Cannot fill data to an empty buffer.");
            }

            if (length > (GetLength() - sourceIndex))
            {
                throw new ArgumentOutOfRangeException("The length should below the size of the buffer.");
            }

            Array.Copy(sourceArray, sourceIndex, Bytes, TotalWriteBytes, length);
            TotalWriteBytes += length;
            Console.WriteLine("Total Bytes Written Into the buffer: {0}", TotalWriteBytes);
        }

        public void CopyTo(byte[] destinationArray, long destinationIndex, long length)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BufferObject));
            }

            if (destinationArray == null)
            {
                throw new ArgumentNullException(nameof(destinationArray));
            }

            if (GetLength() == 0)
            {
                throw new ArgumentException("Cannot fill data to an empty buffer.");
            }

            if (length > (GetLength() - destinationIndex))
            {
                throw new ArgumentOutOfRangeException("The length should below the size of the buffer.");
            }

            Array.Copy(Bytes, TotalReadBytes, destinationArray, destinationIndex, length);
            TotalReadBytes += length;
            Console.WriteLine("Total bytes red from the buffer: {0}", TotalReadBytes);
        }
        #endregion

        public void Dispose()
        {
            Bytes = null;
            IsDisposed = true;
            TotalReadBytes = 0;
            TotalWriteBytes = 0;
            GC.SuppressFinalize(this);
        }
    }
}