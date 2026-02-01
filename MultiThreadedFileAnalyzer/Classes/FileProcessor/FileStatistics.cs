using MultiThreadedFileAnalyzer.Interfaces;

namespace MultiThreadedFileAnalyzer.Classes.FileProcessor;

class FileStatistics : IFileStatistics
{
    public string FileName { get; set; }
    public int LinesCount { get; set; }
    public int WordsCount { get; set; }
    public int CharactersCount { get; set; }

    public FileStatistics()
    {
        FileName = string.Empty;
        LinesCount = 0;
        WordsCount = 0;
        CharactersCount = 0;
    }

    public FileStatistics(string fileName)
    {
        FileName = fileName;
        LinesCount = 0;
        WordsCount = 0;
        CharactersCount = 0;
    }

    public FileStatistics(string fileName, int linesCount, int wordsCount, int charactersCount)
    {
        FileName = fileName;
        LinesCount = linesCount;
        WordsCount = wordsCount;
        CharactersCount = charactersCount;
    }
}

public class FileTask
{
    public string Name { get; set; }
    public int RetryCount { get; set; } = 0;

    public FileTask(string name) => Name = name;
}

