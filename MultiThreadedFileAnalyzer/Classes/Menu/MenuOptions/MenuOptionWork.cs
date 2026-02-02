using Spectre.Console;
using MultiThreadedFileAnalyzer.Classes.App;
using MultiThreadedFileAnalyzer.Classes.FileProcessor;
using MultiThreadedFileAnalyzer.Interfaces;
using System.Collections.Concurrent;

using FileProcessorClass = MultiThreadedFileAnalyzer.Classes.FileProcessor.FileProcessor;

namespace MultiThreadedFileAnalyzer.Classes.Menu.MenuOptions;

internal class MenuOptionWork : MenuOption<MenuOptionWorkParams>
{
    private MenuOptionWorkParams _params;
    public MenuOptionWork(string text) : base(text) { }
    public override void AddParams(MenuOptionWorkParams @params)
    {
        _params = @params;
    }
    public override void Execute()
    {
        _params.appLayout.ShowCurrentDirectory();
        string directoryPath = _params.userPromptDirectory.Prompt();

        if (directoryPath == string.Empty)
        {
            _params.appLayout.ShowErrorMessage($"No directory path was provided.");
            return;
        }

        _params.fileProcessor.DirectoryPath = directoryPath;
        string[]? fileNames = null;
        try
        {
            fileNames = _params.fileProcessor.FindAllTxtFiles(directoryPath);
        }
        catch (Exception ex)
        {
            _params.appLayout.ShowErrorMessage($"{ex.Message}");
            return;
        }
        if (fileNames is null)
        {
            _params.appLayout.ShowErrorMessage($"No files founded in provided directory");
            return;
        }

        ConcurrentStack<FileTask>? stackOfFileTasks = _params.fileStatisticsManager.PutFilesIntoStack(directoryPath, fileNames);
        if (stackOfFileTasks is null)
        {
            _params.appLayout.ShowErrorMessage($"No stack was created from files");
            return;
        }

        Tree tree = _params.appLayout.GetTreeFromFiles(directoryPath, stackOfFileTasks);
        _params.appLayout.RenderTree(tree, "Найденные файлы");

        int numberOfThreads = _params.userPromptThreads.Prompt();

        Semaphore semaphore = new Semaphore(0, numberOfThreads);
        semaphore.Release(numberOfThreads);

        List<IFileStatistics> fileStatisticsList = _params.fileProcessor.ProcessFilesInParallel(
            semaphore, directoryPath, _params.fileStatisticsManager, stackOfFileTasks, numberOfThreads
            );


        Table resultTable = _params.appLayout.CreateResultTable(fileStatisticsList);

        _params.appLayout.RenderResultTable(resultTable, "Результат (возможно не все влезло)");
        AnsiConsole.Write(resultTable);
    }
}

class MenuOptionWorkParams : IMenuOptionParams
{
    public AppLayout appLayout { get; set; }
    public UserPromptDirectory userPromptDirectory { get; set; }
    public UserPromptThreads userPromptThreads { get; set; }
    public FileProcessorClass fileProcessor { get; set; }
    public FileStatisticsManager fileStatisticsManager { get; set; }

    public MenuOptionWorkParams(
        AppLayout appLayout,
        UserPromptDirectory userPromptDirectory,
        UserPromptThreads userPromptThreads,
        FileProcessorClass fileProcessor,
        FileStatisticsManager fileStatisticsManager)
    {
        this.appLayout = appLayout;
        this.userPromptDirectory = userPromptDirectory;
        this.userPromptThreads = userPromptThreads;
        this.fileProcessor = fileProcessor;
        this.fileStatisticsManager = fileStatisticsManager;
    }
}
