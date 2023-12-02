using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace HDF5CSharp.Example.DataTypes
{
    public class BlockingCollectionQueue<T> : IDisposable
    {
        private BlockingCollection<T> items = new BlockingCollection<T>();
        private long runningIndex;

        public int Count => items.Count;

        public void CompleteAdding() => items.CompleteAdding();
        public void Enqueue(T item)
        {
            Interlocked.Increment(ref runningIndex);
            items.Add(item);
        }

        public long RunningIndex => runningIndex;
        public IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken)
        {
            foreach (var item in items.GetConsumingEnumerable())
            {
                yield return item;
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }
            }
        }
        public IEnumerable<T> GetConsumingEnumerable()
        {
            foreach (var item in items.GetConsumingEnumerable())
            {
                yield return item;
            }
        }

        public void Dispose()
        {
            items?.Dispose();
        }
    }
}