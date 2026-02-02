using MultiThreadedFileAnalyzer.Interfaces;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MultiThreadedFileAnalyzer.Classes.Menu;

internal abstract class MenuOption : IExecutable
{
    public string Text { get; set; }
    public MenuOption(string text) => Text = text;
    public abstract void Execute();
    public override string ToString() => $"  [ > ] {Text}";
}

internal abstract class MenuOption<T> : MenuOption where T : IMenuOptionParams
{
    public MenuOption(string text) : base(text) { }
    abstract public void AddParams(T paramsObj);
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
            .BorderColor(Color.Grey35)
            .Expand()
            .Title("[bold yellow]ДОСТУПНЫЕ ДЕЙСТВИЯ[/]")
            .Caption("[grey]Используйте цифры для навигации[/]");

        if (_options == null || _options.Count == 0) 
            return new Panel("[red]Нет опций[/]").Expand();

        table.AddColumn(new TableColumn("[bold]№[/]").Centered().Width(3));
        table.AddColumn(new TableColumn("[bold]КОМАНДА[/]"));

        for (int i = 0; i < _options.Count; i++)
        {
            var color = i % 2 == 0 ? "white" : "cyan1";
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
