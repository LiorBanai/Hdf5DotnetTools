using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace HDF5CSharp;

public class DataProducerConsumer<T> : IDisposable
{
    private BlockingCollection<T> _queue = new BlockingCollection<T>();
    private readonly Action<T> _action;

    public DataProducerConsumer(Action<T> action)
    {
        _action = action;

        var thread = new Thread(StartConsuming)
        {
            IsBackground = true
        };
        thread.Start();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _queue.Dispose();
        }
    }

    public void Done()
    {
        _queue.CompleteAdding();
    }

    public void Produce(T item)
    {
        _queue.Add(item);
    }

    private void StartConsuming()
    {
        while (!_queue.IsCompleted)
        {
            try
            {
                _queue.TryTake(out T data);
                if (data != null)
                {
                    Debug.WriteLine($"item {_queue.Count + 1} in queue will be processed");
                    _action(data);
                }

            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"Work queue on thread {0} has been closed. {ex}", Thread.CurrentThread.ManagedThreadId);
            }
        }
        IsDone?.Invoke(this, new EventArgs());
        Dispose();
    }

    public event EventHandler IsDone;
}
