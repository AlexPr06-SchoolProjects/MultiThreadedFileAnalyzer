using Microsoft.Extensions.DependencyInjection;
using MultiThreadedFileAnalyzer.Classes.Logs;
using MultiThreadedFileAnalyzer.Constants;
using MultiThreadedFileAnalyzer.Interfaces;
using Spectre.Console;
using System.Collections.Concurrent;

namespace MultiThreadedFileAnalyzer.Classes.FileProcessor;

internal class FileProcessorClass(
        [FromKeyedServices("success")] LogPool successfullLogItemsPool,
        [FromKeyedServices("failed")] LogPool failedLogItemsPool,
        [FromKeyedServices("service")] LogPool serviceLogItemsPool,
        [FromKeyedServices("all")] LogPool allLogItemsPool
    ) : IFileProcessorClass
{

    public string DirectoryPath { get; set; } = string.Empty;

    public List<IFileStatistics> ProcessFilesInParallel(
        Semaphore semaphore, string directoryPath, IFileStatisticsManager fsm,
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
        List<IFileStatistics> fileStatisticsList, IFileStatisticsManager
        fsm, Semaphore semaphore, TimeSpan timeSpan, CountdownEvent countdown)
    {
        try
        {
            while (fileNamesStack.TryPop(out FileTask? task))
            {
                int threadId = Thread.CurrentThread.ManagedThreadId;
                if (semaphore.WaitOne(timeSpan))
                {
                    successfullLogItemsPool.AddItem(new LogItem($"{threadId} начал работу.", AppColors.StartedTask));
                    allLogItemsPool.AddItem(new LogItem($"{threadId} начал работу.", AppColors.StartedTask));

                    try
                    {
                        string filePath = Path.Combine(DirectoryPath, task.Name);
                        IFileStatistics fileStatistics = fsm.Analize(filePath, task.Name);
                        lock (locker)
                        {
                            fileStatisticsList.Add(fileStatistics);
                        }
                        serviceLogItemsPool.AddItem(new LogItem($"[yellow]{threadId}[/] обработал файл [darkorange]{task.Name}[/]", AppColors.Service));

                    }
                    catch (Exception ex)
                    {
                        task.RetryCount++;
                        if (task.RetryCount < 3)
                            fileNamesStack.Push(task);
                        failedLogItemsPool.AddItem(new LogItem($"ERROR in {threadId}: {ex.Message}. Попытка: {task.RetryCount}/3", AppColors.Failure));
                        serviceLogItemsPool.AddItem(new LogItem($"{task.Name} был положен в стек.", AppColors.Service));
                        allLogItemsPool.AddItem(new LogItem($"ERROR in {threadId}: {ex.Message}. Попытка: {task.RetryCount}/3", AppColors.Failure));
                        allLogItemsPool.AddItem(new LogItem($"{task.Name} был положен в стек.", AppColors.Service));
                    }
                    finally
                    {
                        successfullLogItemsPool.AddItem(new LogItem($"{threadId} закончил работу.", AppColors.EndedTask));
                        allLogItemsPool.AddItem(new LogItem($"{threadId} закончил работу.", AppColors.EndedTask));
                        semaphore.Release();
                    }
                }
                else
                {
                    serviceLogItemsPool.AddItem(new LogItem($"{threadId} не дождался своей очереди после 3 попыток.", AppColors.Service));
                    allLogItemsPool.AddItem(new LogItem($"{threadId} не дождался своей очереди после 3 попыток.", AppColors.Service));
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
