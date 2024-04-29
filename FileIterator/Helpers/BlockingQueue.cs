namespace FileIterator.Helpers;

public class BlockingQueue<T>
{
    private readonly Queue<T> _queue = new();
    private readonly object _syncRoot = new();

    public void Enqueue(T item)
    {
        lock (_syncRoot)
        {
            _queue.Enqueue(item);
            Monitor.Pulse(_syncRoot);
        }
    }

    public bool TryDequeue(out T result)
    {
        lock (_syncRoot)
        {
            while (_queue.Count == 0)
            {
                Monitor.Wait(_syncRoot);
            }

            result = _queue.Dequeue();
            return true;
        }
    }
}