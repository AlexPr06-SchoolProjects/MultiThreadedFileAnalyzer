using MultiThreadedFileAnalyzer.Interfaces;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace MultiThreadedFileAnalyzer.Classes.FileProcessor;

internal partial class FileStatisticsManager : IFileStatisticsManager
{
    [GeneratedRegex(@"[\p{L}\d]+(-[\p{L}\d]+)*")]
    private static partial Regex WordRegex();
    public IFileStatistics Analize(string filePath)
        => AnalyzeInternal(filePath, Path.GetFileName(filePath));

    public IFileStatistics Analize(string filePath, string fileName)
        => AnalyzeInternal(filePath, fileName);

    public IFileStatistics AnalyzeInternal(string filePath, string fileName)
    {
        int linesCount = 0, wordsCount = 0, charactersCount = 0;
        var regex = WordRegex();

        using var reader = new StreamReader(filePath);
        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            linesCount++;
            wordsCount += regex.Matches(line).Count;
            charactersCount += CountNonWhiteSpaceChars(line);
        }

        return new FileStatistics(fileName, linesCount, wordsCount, charactersCount);
    }

    public ConcurrentStack<FileTask>? PutFilesIntoStack(string directoryPath, string[] files)
    {
        if (!Directory.Exists(directoryPath))
            return null;

        string fullRootPath = Path.GetFullPath(directoryPath);
        ConcurrentStack<FileTask> stack = new ConcurrentStack<FileTask>();
        foreach (string relativePath in files)
        {
            string fullPath = Path.Combine(fullRootPath, relativePath);
            if (File.Exists(fullPath))
            {
                stack.Push(new FileTask(relativePath));
            }
        }

        return stack;
    }

    private static int CountNonWhiteSpaceChars(string line)
    {
        int count = 0;
        foreach (char c in line)
        {
            if (!char.IsWhiteSpace(c)) count++;
        }
        return count;
    }
}

