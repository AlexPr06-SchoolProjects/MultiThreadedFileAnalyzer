using MultiThreadedFileAnalyzer.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadedFileAnalyzer.Classes.App
{
    internal class AppCleaner
    {
        ConcurrentStack<ICleanable> _items;

        public AppCleaner() {
            _items = new ConcurrentStack<ICleanable>();
        }

        public void AddItem(ICleanable item) => _items.Push(item);
        public void AddSome(List<ICleanable> itemsToAdd)
        {
            foreach (var item in itemsToAdd)
                AddItem(item);
        }


        public void CleanAll()
        {
            while (_items.TryPop(out var item)) {
                item.Clean();
            }
        }
    }
}
