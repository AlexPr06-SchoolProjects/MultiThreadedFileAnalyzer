using Spectre.Console.Rendering;

namespace MultiThreadedFileAnalyzer.Interfaces;

internal interface ILoggable
{
    IRenderable Log();
}
