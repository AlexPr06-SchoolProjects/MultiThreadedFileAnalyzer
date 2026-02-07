using MultiThreadedFileAnalyzer.Classes.FileProcessor;
using System.Collections.Concurrent;

namespace MultiThreadedFileAnalyzer.Interfaces;

internal interface IFileStatisticsManager : IAnalizable
{
    public IFileStatistics AnalyzeInternal(string filePath, string fileName);

    public ConcurrentStack<FileTask>? PutFilesIntoStack(string directoryPath, string[] files);
}
