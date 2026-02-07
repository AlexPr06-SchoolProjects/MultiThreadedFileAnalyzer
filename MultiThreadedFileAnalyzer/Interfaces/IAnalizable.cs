namespace MultiThreadedFileAnalyzer.Interfaces;

internal interface IAnalizable
{
    IFileStatistics Analize(string filePath);
    IFileStatistics Analize(string filePath, string fileName);
}

