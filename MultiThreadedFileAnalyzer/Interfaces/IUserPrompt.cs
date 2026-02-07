namespace MultiThreadedFileAnalyzer.Interfaces;

internal interface IUserPrompt<out T>
{
    T Prompt();
}
