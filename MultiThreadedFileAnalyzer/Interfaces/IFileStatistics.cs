namespace MultiThreadedFileAnalyzer.Interfaces;

internal interface IFileStatistics
{
    public string FileName { get; set; }
    public int LinesCount { get; set; }
    public int WordsCount { get; set; }
    public int CharactersCount { get; set; }
}

