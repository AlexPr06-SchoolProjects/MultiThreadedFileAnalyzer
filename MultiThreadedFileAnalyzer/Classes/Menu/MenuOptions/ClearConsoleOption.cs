using Spectre.Console;

namespace MultiThreadedFileAnalyzer.Classes.Menu.MenuOptions;

internal sealed class ClearConsoleOption(string text) : MenuOption(text)
{
    public override void Execute()
    {
        AnsiConsole.Clear();
    }
}
