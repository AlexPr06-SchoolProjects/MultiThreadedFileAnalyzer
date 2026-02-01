using MultiThreadedFileAnalyzer.Interfaces;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Collections.Concurrent;

namespace MultiThreadedFileAnalyzer.Classes;

internal abstract class MenuOption : IExecutable
{
    public string Text { get; set; }
    public MenuOption(string text) => Text = text;
    public abstract void Execute();
    public override string ToString() => $"  [ > ] {Text}";
}

internal abstract class MenuOption<T> : MenuOption where T : IMenuOptionParams
{
    protected T _params;
    public MenuOption(string text) : base(text) { }
    abstract public void AddParams(T paramsObj);
}


class MenuOptionWorkParams : IMenuOptionParams
{
    public AppLayout appLayout { get; set; }
    public UserPromptDirectory userPromptDirectory { get; set; }
    public UserPromptThreads userPromptThreads { get; set; }
    public FileProcessor fileProcessor { get; set; }
    public FileStatisticsManager fileStatisticsManager { get; set; }

    public MenuOptionWorkParams(
        AppLayout appLayout, 
        UserPromptDirectory userPromptDirectory,
        UserPromptThreads userPromptThreads,
        FileProcessor fileProcessor,
        FileStatisticsManager fileStatisticsManager)
    {
        this.appLayout = appLayout;
        this.userPromptDirectory = userPromptDirectory;
        this.userPromptThreads = userPromptThreads;
        this.fileProcessor = fileProcessor;
        this.fileStatisticsManager = fileStatisticsManager;
    }
}

internal class MenuOptionWork : MenuOption<MenuOptionWorkParams>
{
    private MenuOptionWorkParams _menuOptionWorkParams;
    public MenuOptionWork(string text) : base(text) {}
    public override void AddParams(MenuOptionWorkParams @params)
    {
        _menuOptionWorkParams = @params;
    }
    public override void Execute()
    {
        _menuOptionWorkParams.appLayout.ShowCurrentDirectory();
        string directoryPath = _menuOptionWorkParams.userPromptDirectory.Prompt();

        if (directoryPath == String.Empty)
        {
            _menuOptionWorkParams.appLayout.ShowErrorMessage($"No directory path was provided.");
            return;
        }

        _menuOptionWorkParams.fileProcessor.DirectoryPath = directoryPath;
        int numberOfThreads = _menuOptionWorkParams.userPromptThreads.Prompt();

        string[]? fileNames = _menuOptionWorkParams.fileProcessor.FindAllTxtFiles(directoryPath);
        if (fileNames is null)
        {
            _menuOptionWorkParams.appLayout.ShowErrorMessage($"No files founded in provided directory");
            return;
        }

        ConcurrentStack<FileTask>? stackOfFileTasks = _menuOptionWorkParams.fileStatisticsManager.PutFilesIntoStack(directoryPath, fileNames);
        if (stackOfFileTasks is null)
        {
            _menuOptionWorkParams.appLayout.ShowErrorMessage($"No stack was created from files");
            return;
        }

        foreach (var fileTask in stackOfFileTasks)
            Console.WriteLine($"{fileTask.Name}");
 
        Semaphore semaphore = new Semaphore(0, numberOfThreads);
        semaphore.Release(numberOfThreads);

        var fileStatisticsList = _menuOptionWorkParams.fileProcessor.ProcessFilesInParallel(
            semaphore, directoryPath, _menuOptionWorkParams .fileStatisticsManager, stackOfFileTasks, numberOfThreads
            );

        foreach (var file in fileStatisticsList)
            Console.WriteLine($"{file.FileName}: Lines:{file.LinesCount} WordsCount: {file.WordsCount} CharactersCount: {file.CharactersCount}");

    }
}
internal class Menu : IOwnRenderable
{
    private List<MenuOption> _options;
    public Menu() {
        _options = new List<MenuOption>();
    }
    public Menu(List<MenuOption> options)
    {
        _options = options;
    }
    public void AddOption(MenuOption option) => _options.Add(option);

    public void Init(List<MenuOption> menuOptions)
    {
        foreach (var option in menuOptions)
            _options.Add(option);
    }


    public IRenderable OwnRender()
    {
        var table = new Table()
            .RoundedBorder()
            .BorderColor(Spectre.Console.Color.Grey35)
            .Expand()
            .Title("[bold yellow]ДОСТУПНЫЕ ДЕЙСТВИЯ[/]")
            .Caption("[grey]Используйте цифры для навигации[/]");

        if (_options == null || _options.Count == 0) 
            return new Panel("[red]Нет опций[/]").Expand();

        table.AddColumn(new TableColumn("[bold]№[/]").Centered().Width(3));
        table.AddColumn(new TableColumn("[bold]КОМАНДА[/]"));

        for (int i = 0; i < _options.Count; i++)
        {
            var color = (i % 2 == 0) ? "white" : "cyan1";
            table.AddRow($"[grey]{i + 1}[/]", $"[{color}]{_options[i].Text}[/]");
        }

        return table;
    }

    public int AskForMenuOption(int menuOptionsAmount)
    {
        var menuOptionPrompt = new TextPrompt<int>("Выберите команду: ");

        for (int i = 1; i <= menuOptionsAmount; i++)
        {
            menuOptionPrompt.AddChoice(i);
        }

        menuOptionPrompt.ShowChoices(true);

        return AnsiConsole.Prompt(menuOptionPrompt);
    }
}
