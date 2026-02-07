using MultiThreadedFileAnalyzer.Classes.Menu;

namespace MultiThreadedFileAnalyzer.Interfaces;

internal interface IMenuOptionFactory
{
    public MenuOption CreateWorkOption(string title);
    public MenuOption CreateClearOption(string title);
}
