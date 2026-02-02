using MultiThreadedFileAnalyzer.Interfaces;
using Spectre.Console;

namespace MultiThreadedFileAnalyzer.Classes.Menu.MenuOptions;

internal class ClearConsoleOption : MenuOption<ClearConsoleOptionParams>
{
    private ClearConsoleOptionParams _params;
    public ClearConsoleOption(string text) : base(text) { }
    public override void AddParams(ClearConsoleOptionParams @params)
    {
        _params = @params;
    }

    public override void Execute()
    {
        AnsiConsole.Clear();
    }
}

internal class ClearConsoleOptionParams : IMenuOptionParams 
{
    public ClearConsoleOptionParams() { }
}

