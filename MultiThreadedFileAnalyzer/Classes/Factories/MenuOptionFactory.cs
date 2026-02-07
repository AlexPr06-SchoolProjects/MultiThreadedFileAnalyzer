using Microsoft.Extensions.DependencyInjection;
using MultiThreadedFileAnalyzer.Classes.App;
using MultiThreadedFileAnalyzer.Classes.Menu;
using MultiThreadedFileAnalyzer.Classes.Menu.MenuOptions;
using MultiThreadedFileAnalyzer.Interfaces;

namespace MultiThreadedFileAnalyzer.Classes.Factories;

internal sealed class MenuOptionFactory(IServiceProvider sp) : IMenuOptionFactory
{
    public MenuOption CreateWorkOption(string title) {

        return new MenuOptionWork(
            title,
            sp.GetRequiredService<AppLayout>(),
            sp.GetRequiredKeyedService<IUserPrompt<string>>("UserPromptDirectory"),
            sp.GetRequiredKeyedService<IUserPrompt<int>>("UserPromptThreads"),
            sp.GetRequiredKeyedService<IFileProcessorClass>("FileProcessorClass"),
            sp.GetRequiredKeyedService<IFileStatisticsManager>("FileStatisticsManager")
        );
    }
    public MenuOption CreateClearOption(string title) => new ClearConsoleOption(title);
}
