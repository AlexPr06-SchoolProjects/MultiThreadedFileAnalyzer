using Spectre.Console;
using MultiThreadedFileAnalyzer.Interfaces;
using Spectre.Console.Rendering;

namespace MultiThreadedFileAnalyzer.Classes.Logs;

internal sealed class LogItem : ILoggable
{
    public Color OutputColor { get; set; }
    public string Message { get; set; }

    public LogItem(string message, Color outputColor)
    {
        Message = message;
        OutputColor = outputColor;
    }

    public IRenderable Log()
    {
        var panel = new Panel(Message)
        {
            Header = new PanelHeader("Log Entry"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(OutputColor)
        };
        return panel;
    }
}

