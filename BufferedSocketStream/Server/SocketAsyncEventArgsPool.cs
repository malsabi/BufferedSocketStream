using System.Net.Sockets;
using System.Collections.Concurrent;

namespace BufferedSocketStream.Server
{
    public sealed class SocketAsyncEventArgsPool
    {
        /// <summary>
        /// Pool of SocketAsyncEventArgs.
        /// </summary>
        public ConcurrentStack<Tuple<SocketAsyncEventArgs, SocketAsyncEventArgs>> Pool { get; private set; }

        /// <summary>
        /// Represents the maximum number of SocketAsyncEventArgs objects the pool can hold.
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// Initializes the object pool to the specified size.
        /// </summary>
        /// <param name="Capacity"> Maximum number of SocketAsyncEventArgs objects the pool can hold.</param>
        public SocketAsyncEventArgsPool(int Capacity)
        {
            this.Capacity = Capacity;
            Pool = new ConcurrentStack<Tuple<SocketAsyncEventArgs, SocketAsyncEventArgs>>();
        }

        /// <summary>
        /// Removes a SocketAsyncEventArgs instance from the pool.
        /// </summary>
        /// <returns>SocketAsyncEventArgs removed from the pool.</returns>
        public Tuple<SocketAsyncEventArgs, SocketAsyncEventArgs> Pop()
        {
            if (Pool == null)
            {
                return null;
            }
            if (Pool.TryPop(out var e))
            {
                return e;
            }
            return null;
        }

        /// <summary>
        /// Add a SocketAsyncEventArg instance to the pool. 
        /// </summary>
        /// <param name="item">SocketAsyncEventArgs instance to add to the pool.</param>
        public void Push(Tuple<SocketAsyncEventArgs, SocketAsyncEventArgs> item)
        {
            if (item == null)
            {
                throw new Exception("Item cannot be null.");
            }
            if (Pool?.Count >= Capacity)
            {
                throw new Exception("Cannot push more than the allowed pool capacity.");
            }
            Pool?.Push(item);
        }

        /// <summary>
        /// Clears the <see cref="Pool"/>.
        /// </summary>
        public void ClearPool()
        {
            Pool?.Clear();
        }
    }
}
