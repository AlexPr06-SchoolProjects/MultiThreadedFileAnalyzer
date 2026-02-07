using Spectre.Console;
using MultiThreadedFileAnalyzer.Classes.App;
using MultiThreadedFileAnalyzer.Classes.FileProcessor;
using MultiThreadedFileAnalyzer.Interfaces;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace MultiThreadedFileAnalyzer.Classes.Menu.MenuOptions;

internal sealed class MenuOptionWork (
        string text,
        AppLayout appLayout,
        [FromKeyedServices("UserPromptDirectory")] IUserPrompt<string> userPromptDirectory,
        [FromKeyedServices("UserPromptThreads")] IUserPrompt<int> userPromptThreads,
        [FromKeyedServices("FileProcessorClass")] IFileProcessorClass fileProcessor,
        IFileStatisticsManager fileStatisticsManager
    ) : MenuOption(text)
{
    public override bool NeedsLogRendering => true;
    public override void Execute()
    {
        appLayout.ShowCurrentDirectory();
        string directoryPath = userPromptDirectory.Prompt();

        try
        {
            if (directoryPath == string.Empty)
            {
                appLayout.ShowErrorMessage($"No directory path was provided.");
                return;
            }

            fileProcessor.DirectoryPath = directoryPath;

            var fileNames = fileProcessor.FindAllTxtFiles(directoryPath);
            if (fileNames is null || fileNames.Length == 0)
            {
                appLayout.ShowErrorMessage("No files found in provided directory");
                return;
            }

            ConcurrentStack<FileTask>? stackOfFileTasks = fileStatisticsManager.PutFilesIntoStack(directoryPath, fileNames);
            if (stackOfFileTasks is null)
            {
                appLayout.ShowErrorMessage($"No stack was created from files");
                return;
            }

            Tree tree = appLayout.GetTreeFromFiles(directoryPath, stackOfFileTasks);
            appLayout.RenderTree(tree, "Найденные файлы");

            int numberOfThreads = userPromptThreads.Prompt();

            using Semaphore semaphore = new Semaphore(0, numberOfThreads);
            semaphore.Release(numberOfThreads);

            List<IFileStatistics> fileStatisticsList = fileProcessor.ProcessFilesInParallel(
                semaphore, directoryPath, fileStatisticsManager, stackOfFileTasks, numberOfThreads
                );


            Table resultTable = appLayout.CreateResultTable(fileStatisticsList);

            appLayout.RenderResultTable(resultTable, "Результат (возможно не все влезло)");
            AnsiConsole.Write(resultTable);
        } catch (Exception ex)
        {
            appLayout.ShowErrorMessage($"{ex.Message}");
        }
    }
}
