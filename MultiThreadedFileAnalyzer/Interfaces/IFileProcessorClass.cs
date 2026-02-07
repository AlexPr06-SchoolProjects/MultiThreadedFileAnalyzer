using MultiThreadedFileAnalyzer.Classes.FileProcessor;
using MultiThreadedFileAnalyzer.Classes.Logs;
using MultiThreadedFileAnalyzer.Constants;
using System.Collections.Concurrent;

namespace MultiThreadedFileAnalyzer.Interfaces;

internal interface IFileProcessorClass
{
    public string DirectoryPath { get; set; }
    public List<IFileStatistics> ProcessFilesInParallel(
            Semaphore semaphore, string directoryPath, IFileStatisticsManager fsm,
            ConcurrentStack<FileTask> fileNamesStack, int maxThreads);

    public string[]? FindAllTxtFiles(string directoryPath);
}
