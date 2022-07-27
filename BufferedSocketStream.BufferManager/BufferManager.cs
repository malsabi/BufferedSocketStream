using System;
using System.Linq;
using System.Collections.Concurrent;

namespace BufferedSocketStream.BufferManager
{
    /// <summary>
    /// <para>
    /// Represents a pool of buffers that can be adjusted by the <see cref="BufferManager"/> constructor which
    /// accepts two actual parameters, <see cref="BufferCount"/> and <see cref="BufferCount"/>. The buffer count specifies how many buffers
    /// should be added into the buffer pool or the capacity of the buffer pool. The buffer length specifies the
    /// size of each buffer that will be added into the pool.
    /// </para>
    /// <para>
    /// The buffer is an object that is created from <see cref="BufferObject"/> which creates an internal byte
    /// array by a given size which is the <see cref="BufferLength"/>. The <see cref="BufferObject"/> provides multiple useful 
    /// methods but most importantly it provides a decent way to dispose it self and clears the internal byte array 
    /// from the memory.
    /// </para>
    /// <para>
    /// NOTE: Do not call <see cref="BufferObject.Dispose"/> method in both cases returned or not returned to the
    /// <see cref="BufferManager"/>. The Dispose method is being used only when the <see cref="Clear"/> method is
    /// called.
    /// </para>
    /// <para>
    /// NOTE: Do not call <see cref="BufferObject.Dispose"/> method in case of a shallow copy of the buffer object,
    /// but it can be called in case of a deep copy (creating a new reference and copy it's content), dispose the
    /// cloned object and return the main object to the buffer pool.
    /// </para>
    /// </summary>
    public class BufferManager
    {
        #region "Fields"
        private ConcurrentStack<BufferObject> bufferPool;
        #endregion

        #region "Properties"
        /// <summary>
        /// Gets the number of maximum buffers should be added into the pool or the capacity of the pool.
        /// </summary>
        public int BufferCount { get; private set; }

        /// <summary>
        /// Gets the number of maximum bytes should be allocated for a single buffer that will be added into the pool.
        /// </summary>
        public int BufferLength { get; private set; }

        /// <summary>
        /// Gets the number of available buffers from the pool.
        /// </summary>
        public int AvailableBuffers { get => bufferPool == null ? 0 : bufferPool.Count; }
        #endregion

        /// <summary>
        /// Initializes the stack pool by setting it's capacity to the <see cref="BufferCount"/> and each buffer 
        /// that will be pushed into the stack will contain a size of <see cref="BufferLength"/>.
        /// </summary>
        /// <param name="bufferCount">Represents the capacity of the <see cref="StackPool"/>.</param>
        /// <param name="bufferLength">Represents the buffer item size that will be pushed into the <see cref="StackPool"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="BufferCount"/> or <see cref="BufferLength"/> is set to zero or below.</exception>
        public BufferManager(int bufferCount, int bufferLength)
        {
            if (bufferCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferCount), bufferCount, "Buffer count must be a positive integer value.");
            }

            if (bufferLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferLength), bufferLength, "Buffer length must be a positive integer value.");
            }

            bufferPool = new ConcurrentStack<BufferObject>();

            BufferCount = bufferCount;
            BufferLength = bufferLength;

            for (int i = 0; i < bufferCount; i++)
            {
                bufferPool.Push(new BufferObject(bufferLength));
            }
        }

        /// <summary>
        /// <para>
        /// Gets a buffer sigment from the buffer pool, the returned buffer size is based on the <see cref="BufferLength"/>.
        /// </para>
        /// <para>
        /// Make sure to return the buffer to the buffer pool by using <see cref="SetBuffer(BufferObject, bool)"/> method
        /// and clear any object that is referencing the buffer before returning it.
        /// </para>
        /// </summary>
        /// <returns>A buffer sigment from the buffer pool.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws an exception if the buffer pool is empty.</exception>
        /// <exception cref="InvalidOperationException">Throws an exception if the buffer pool has no available buffers to retrieve.</exception>
        public BufferObject GetBuffer()
        {
            if (bufferPool.Count == 0)
            {
                throw new ArgumentOutOfRangeException("The stack pool is empty and cannot retrive a bufffer.");
            }
            if (!bufferPool.TryPop(out BufferObject buffer))
            {
                throw new InvalidOperationException("Failed on getting a buffer slab from the BufferPool.");
            }
            return buffer;
        }

        /// <summary>
        /// Sets the buffer that was taken by using <see cref="GetBuffer"/> method into the buffer pool for reusing it again.
        /// </summary>
        /// <param name="buffer">Represents the buffer that was taken by using <see cref="GetBuffer"/> method.</param>
        /// <param name="clearBufferOnReturn">True to clear the buffer contents or False to ignore clearing the buffer contents.</param>
        /// <returns>Returns true if the buffer was successfully added to the buffer pool otherwise it will return false.</returns>
        /// <exception cref="ArgumentNullException">Throws an exception if the buffer given was null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws an exception if the buffer given was not the same size of the <see cref="BufferLength"/>.</exception>
        public bool SetBuffer(BufferObject buffer, bool clearBufferOnReturn = true)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (buffer.Bytes.Length != BufferLength)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer.Bytes.Length), buffer.Bytes.Length, "The passed buffer must be the same as the BufferLength that was set initially.");
            }
            if (clearBufferOnReturn)
            {
                Array.Clear(buffer.Bytes, 0, buffer.Bytes.Length);
            }
            if (!bufferPool.Contains(buffer))
            {
                bufferPool.Push(buffer);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clears the buffer pool if it is not empty and releases all managed resourcs.
        /// </summary>
        public void Clear()
        {
            if (!bufferPool.IsEmpty)
            {
                foreach (BufferObject buffer in bufferPool)
                {
                    buffer.Dispose();
                }
                bufferPool.Clear();
            }
            bufferPool = null;
        }
    }
}