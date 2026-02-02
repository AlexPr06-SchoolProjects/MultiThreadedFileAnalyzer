using MultiThreadedFileAnalyzer.Classes.Logs;
using MultiThreadedFileAnalyzer.Constants;
using MultiThreadedFileAnalyzer.Interfaces;
using Spectre.Console;
using System.Collections.Concurrent;

namespace MultiThreadedFileAnalyzer.Classes.FileProcessor
{
    internal class FileProcessor
    {
        private LogPool _failedItemsPool;
        private LogPool _successfullItemsPool;
        private LogPool _serviceItemsPool;
        private LogPool _allLogs;
        public string DirectoryPath {  get; set; }

        public FileProcessor(LogPool failedItemsPool, LogPool successfullItemsPool, LogPool serviceItemsPool, LogPool allLogs, string directoryPath)
        {
            _failedItemsPool = failedItemsPool;
            _successfullItemsPool = successfullItemsPool;
            _serviceItemsPool = serviceItemsPool;
            _allLogs = allLogs;
            DirectoryPath = directoryPath;
        }


        public List<IFileStatistics> ProcessFilesInParallel(
            Semaphore semaphore, string directoryPath, FileStatisticsManager fsm,
            ConcurrentStack<FileTask> fileNamesStack, int maxThreads)
        {
            List<IFileStatistics> fileStatisticsList = new List<IFileStatistics>();
            object locker = new object();
            TimeSpan timeSpan = TimeSpan.FromSeconds(5);

            int threadsToStart = Math.Min(maxThreads, fileNamesStack.Count);

            using (CountdownEvent countdown = new CountdownEvent(threadsToStart))
            {
                for (int i = 0; i < threadsToStart; ++i)
                {
                    new Task(() =>
                    {
                        ProcessFile(
                            fileNamesStack, locker, fileStatisticsList,
                            fsm, semaphore, timeSpan, countdown);
                    }).Start();
                }
                countdown.Wait();
            };

            return fileStatisticsList;
        }

        private void ProcessFile(
            ConcurrentStack<FileTask> fileNamesStack, object locker,
            List<IFileStatistics> fileStatisticsList, FileStatisticsManager
            fsm, Semaphore semaphore, TimeSpan timeSpan, CountdownEvent countdown)
        {
            try
            {
                while (fileNamesStack.TryPop(out FileTask? task))
                {
                    int threadId = Thread.CurrentThread.ManagedThreadId;
                    if (semaphore.WaitOne(timeSpan))
                    {
                        _successfullItemsPool.AddItem(new LogItem($"{threadId} начал работу.", AppColors.StartedTask));
                        _allLogs.AddItem(new LogItem($"{threadId} начал работу.", AppColors.StartedTask));

                        try
                        {
                            string filePath = Path.Combine(DirectoryPath, task.Name);
                            IFileStatistics fileStatistics = fsm.Analize(filePath, task.Name);
                            lock (locker)
                            {
                                fileStatisticsList.Add(fileStatistics);
                            }
                            _serviceItemsPool.AddItem(new LogItem($"[yellow]{threadId}[/] обработал файл [darkorange]{task.Name}[/]", AppColors.Service));

                        }
                        catch (Exception ex)
                        {
                            task.RetryCount++;
                            if (task.RetryCount < 3)
                                fileNamesStack.Push(task);
                            _failedItemsPool.AddItem(new LogItem($"ERROR in {threadId}: {ex.Message}. Попытка: {task.RetryCount}/3", AppColors.Failure));
                            _serviceItemsPool.AddItem(new LogItem($"{task.Name} был положен в стек.", AppColors.Service));
                            _allLogs.AddItem(new LogItem($"ERROR in {threadId}: {ex.Message}. Попытка: {task.RetryCount}/3", AppColors.Failure));
                            _allLogs.AddItem(new LogItem($"{task.Name} был положен в стек.", AppColors.Service));
                        }
                        finally
                        {
                            _successfullItemsPool.AddItem(new LogItem($"{threadId} закончил работу.", AppColors.EndedTask));
                            _allLogs.AddItem(new LogItem($"{threadId} закончил работу.", AppColors.EndedTask));
                            semaphore.Release();
                        }
                    }
                    else
                    {
                        _serviceItemsPool.AddItem(new LogItem($"{threadId} не дождался своей очереди после 3 попыток.", AppColors.Service));
                        _allLogs.AddItem(new LogItem($"{threadId} не дождался своей очереди после 3 попыток.", AppColors.Service));
                    }
                }
            }
            finally
            {
                countdown.Signal();
            }
        }

        // Try-catch wrapper required(i.e. access is denied)
        public string[]? FindAllTxtFiles(string directoryPath)
        {
                if (!Directory.Exists(directoryPath))
                    return null;

                string rootPath = Path.GetFullPath(directoryPath);
                var fullPaths = Directory.GetFiles(rootPath, "*.txt", SearchOption.AllDirectories);
                return fullPaths.Select(path => Path.GetRelativePath(rootPath, path)).ToArray();
        }
    }
}
