using MultiThreadedFileAnalyzer.Interfaces;
using System.Collections.Concurrent;

namespace MultiThreadedFileAnalyzer.Classes;
internal class LogPool
{
    public readonly ConcurrentQueue<ILoggable> _loggableItems;

    public LogPool() {
        _loggableItems = new ConcurrentQueue<ILoggable>();
    }

    public void AddItem(ILoggable item) =>  _loggableItems.Enqueue(item);

    public void Show()
    {
        foreach (var item in _loggableItems)
        {
            item.Log();
        }
    }

    public void ShowItem()
    {
        if (_loggableItems.TryDequeue(out var item))
            item.Log();
    }

    public void Clear() => _loggableItems.Clear();

}
