namespace BufferedSocketStream.BufferManager
{
    /// <summary>
    /// <para>
    /// Represents a wrapper for the buffer which is a byte array that is allocated internally 
    /// by the given length of <see cref="BufferSize"/>
    /// </para>
    /// <para>
    /// Provides useful methods for filling the buffer or copying the buffer content into another
    /// destination, also provides a dispose method for clearing the buffer so the Garbage Collector (GC)
    /// can collect the buffer and retrieve a free space in the memory.
    /// </para>
    /// </summary>
    public class BufferObject : IDisposable
    {
        #region "Fields"
        private byte[] buffer;
        #endregion

        #region "Properties"
        /// <summary>
        /// Returns true if the <see cref="Dispose"/> method is called otherwise returns False.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Returns the number of bytes that have been written into the buffer by using the 
        /// <see cref="FillWith(byte[], long, long)"/> method.
        /// </summary>
        public long TotalWriteBytes { get; private set; }

        /// <summary>
        /// Returns the number of bytes that have been read from the buffer by using the 
        /// <see cref="CopyTo(byte[], long, long)"/> method.
        /// </summary>
        public long TotalReadBytes { get; private set; }

        /// <summary>
        /// Returns the allocated buffer size.
        /// </summary>
        public long BufferSize { get; private set; }
        #endregion

        /// <summary>
        /// Allocates the internal byte array by the given buffer size.
        /// </summary>
        /// <param name="bufferSize">Represents the byte array size.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>
        /// Throws an exception if the buffer size was equal or less than zero.
        /// </para>
        /// <para>
        /// Throws an exception if the buffer size exceeded the maximum limit of 150 Kilobytes.
        /// </para>
        /// </exception>
        public BufferObject(int bufferSize)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "buffer size cannot be equal or less than zero");
            }
            if (bufferSize > 150000)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "buffer size cannot exceed the maximum limit of 150KB");
            }
            buffer = new byte[bufferSize];
            BufferSize = bufferSize;
            IsDisposed = false;
        }

        #region "Public Methods"
        /// <summary>
        /// Gets the internal allocated byte array (buffer).
        /// </summary>
        /// <returns>Returns null if the buffer was null otherwise it will return buffer.</returns>
        public byte[] GetBuffer()
        {
            if (buffer == null)
            {
                return null;
            }
            return buffer;
        }

        /// <summary>
        /// Fills the buffer content from another source content.
        /// </summary>
        /// <param name="sourceArray">Represents the source content.</param>
        /// <param name="sourceIndex">Represents the source start index.</param>
        /// <param name="length">Represents the length of how many bytes to copy from source to buffer.</param>
        /// <exception cref="ObjectDisposedException">Throws an exception if the <see cref="IsDisposed"/> was True.</exception>
        /// <exception cref="ArgumentNullException">Throws an exception if the provided source was null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws an exception if the length provided is out of the buffer bounds.</exception>
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

            if (length > (BufferSize - sourceIndex))
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The length should below the size of the buffer.");
            }

            Array.Copy(sourceArray, sourceIndex, buffer, TotalWriteBytes, length);
            TotalWriteBytes += length;
            Console.WriteLine("Total Bytes Written Into the buffer: {0}", TotalWriteBytes);
        }

        /// <summary>
        /// Copies the buffer content into another destination byte array.
        /// </summary>
        /// <param name="destinationArray">Represents the destionation byte array.</param>
        /// <param name="destinationIndex">Represents the destination start index.</param>
        /// <param name="length">Represents the length of how many bytes to copy from the buffer to the destination byte array.</param>
        /// <exception cref="ObjectDisposedException">Throws an exception if the <see cref="IsDisposed"/> was True.</exception>
        /// <exception cref="ArgumentNullException">Thrwos an exception if the destination byte array was null or emoty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws an exception if the length provided is out of the buffer bounds.</exception>
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

            if (length > (BufferSize - destinationIndex))
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The length should below the size of the buffer.");
            }

            Array.Copy(buffer, TotalReadBytes, destinationArray, destinationIndex, length);
            TotalReadBytes += length;
            Console.WriteLine("Total bytses red from the buffer: {0}", TotalReadBytes);
        }

        /// <summary>
        /// Releases all managed resources and sets <see cref="IsDisposed"/> to True.
        /// </summary>
        public void Dispose()
        {
            buffer = null;
            IsDisposed = true;
            TotalReadBytes = 0;
            TotalWriteBytes = 0;
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}