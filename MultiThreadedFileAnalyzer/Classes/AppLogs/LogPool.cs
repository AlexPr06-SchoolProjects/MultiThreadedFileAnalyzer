using MultiThreadedFileAnalyzer.Interfaces;
using System.Collections.Concurrent;

namespace MultiThreadedFileAnalyzer.Classes.Logs;
internal sealed class LogPool : ICleanable
{
    private readonly ConcurrentQueue<ILoggable> _loggableItems = new();

    public void AddItem(ILoggable item) =>  _loggableItems.Enqueue(item);

    public IReadOnlyCollection<ILoggable> AllLogs => _loggableItems;

    public void ShowAll()
    {
        foreach (var item in _loggableItems)
        {
            item.Log();
        }
    }

    public void ShowRecent(int count)
    {
        foreach (var item in _loggableItems.TakeLast(count))
        {
            item.Log();
        }
    }

    public bool TryLogNext()
    {
        if (_loggableItems.TryDequeue(out var item))
        {
            item.Log();
            return true;
        }
        return false;
    }

    public void Clean() => _loggableItems.Clear();
}
