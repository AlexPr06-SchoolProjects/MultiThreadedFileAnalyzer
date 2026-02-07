namespace MultiThreadedFileAnalyzer.Interfaces;

internal interface IFileStatistics
{
    public string FileName { get; init; }
    public int LinesCount { get; init; }
    public int WordsCount { get; init; }
    public int CharactersCount { get; init; }
}

