using MultiThreadedFileAnalyzer.Interfaces;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MultiThreadedFileAnalyzer.Classes.Menu;

internal abstract class MenuOption(string text) : IExecutable
{
    public string Text { get; } = text;
    public abstract void Execute();
    public virtual bool NeedsLogRendering => false;
    public virtual bool NeedsLayoutRefresh => false;
}

internal sealed class MenuClass(IEnumerable<MenuOption> options) : IOwnRenderable
{
    private readonly List<MenuOption> _options = options.ToList();

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
