using MultiThreadedFileAnalyzer.Interfaces;

namespace MultiThreadedFileAnalyzer.Classes.FileProcessor;

internal record FileStatistics (
        string FileName = "",
        int LinesCount = 0,
        int WordsCount = 0,
        int CharactersCount = 0
    ) : IFileStatistics;

public class FileTask(string name)
{
    public string Name { get; set; } = name;
    public int RetryCount { get; set; } = 0;
}

