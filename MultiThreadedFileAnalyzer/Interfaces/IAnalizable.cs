namespace MultiThreadedFileAnalyzer.Interfaces;

internal interface IAnalizable
{
    IFileStatistics Analize(string filePath);
}

