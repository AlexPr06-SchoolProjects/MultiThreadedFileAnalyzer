using MultiThreadedFileAnalyzer.Interfaces;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace MultiThreadedFileAnalyzer.Classes;

internal class FileStatisticsManager : IAnalizable
{
    public IFileStatistics Analize(string filePath)
    {
        IFileStatistics analizedFile = AnalyzeFile(filePath);
        return analizedFile;
    }

    public IFileStatistics Analize(string filePath, string fileName)
    {
        IFileStatistics analizedFile = AnalyzeFile(filePath, fileName);
        return analizedFile;
    }

    private IFileStatistics AnalyzeFile(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        IFileStatistics fileStatistics = new FileStatistics(fileName);
        string pattern = @"[\p{L}\d]+(-[\p{L}\d]+)*";
        Regex regex = new Regex(pattern);
        using (var reader = new StreamReader(filePath))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                fileStatistics.LinesCount++;

                fileStatistics.WordsCount += regex.Matches(line).Count;

                foreach (char c in line)
                {
                    if (!char.IsWhiteSpace(c)) fileStatistics.CharactersCount++;
                }
            }
        }

        return fileStatistics;
    }

    private IFileStatistics AnalyzeFile(string filePath, string fileName) { 
        IFileStatistics fileStatistics = new FileStatistics(fileName);
        string pattern = @"[\p{L}\d]+(-[\p{L}\d]+)*";
        Regex regex = new Regex(pattern);
        using (var reader = new StreamReader(filePath))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                fileStatistics.LinesCount++;

                fileStatistics.WordsCount += regex.Matches(line).Count;

                foreach (char c in line)
                {
                    if (!char.IsWhiteSpace(c)) fileStatistics.CharactersCount++;
                }
            }
        }

        return fileStatistics;
    }

    public string[]? FindAllTxtFiles(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return null;

        string rootPath = Path.GetFullPath(directoryPath);
        var fullPaths = Directory.GetFiles(rootPath, "*.txt", SearchOption.AllDirectories);
        return fullPaths.Select(path => Path.GetRelativePath(rootPath, path)).ToArray();
    }


    public ConcurrentStack<FileTask>? PutFilesIntoStack(string directoryPath, string[] files)
    {
        if (!Directory.Exists(directoryPath))
            return null;

        string rootPath = Path.GetFullPath(directoryPath);
        ConcurrentStack<FileTask> fileNamesStack = new ConcurrentStack<FileTask> { };
        foreach (string relativePath in files)
        {
            string fullPath = Path.Combine(rootPath, relativePath);
            if (File.Exists(fullPath))
            {
                fileNamesStack.Push(new FileTask(relativePath));
            }
        }

        return fileNamesStack;
    }
}

