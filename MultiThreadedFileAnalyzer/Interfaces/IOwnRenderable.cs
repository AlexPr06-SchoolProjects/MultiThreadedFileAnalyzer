using Spectre.Console.Rendering;

namespace MultiThreadedFileAnalyzer.Interfaces;

internal interface IOwnRenderable
{
    public IRenderable OwnRender();
}
