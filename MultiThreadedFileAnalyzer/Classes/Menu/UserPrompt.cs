using MultiThreadedFileAnalyzer.Constants;
using MultiThreadedFileAnalyzer.Interfaces;
using Spectre.Console;

namespace MultiThreadedFileAnalyzer.Classes.Menu;

internal sealed class UserPromptThreads : IUserPrompt<int>
{
    public int Prompt()
    {
        int? threads = AnsiConsole.Prompt(
            new TextPrompt<int?>("Сколько [green]потоков[/] задействовать?")
                .PromptStyle("yellow")
                .ValidationErrorMessage("[red]Ошибка:[/] введите число от 1 до 16")
                .Validate(value => value switch
                {
                    null => ValidationResult.Success(),
                    < ThreadConstraints.THREADS_MIN => ValidationResult.Error($"Слишком мало! Нужно хотя бы {ThreadConstraints.THREADS_MIN}"),
                    > ThreadConstraints.THREADS_MAX => ValidationResult.Error($"Слишком много! Максимум {ThreadConstraints.THREADS_MAX}"),
                    _ => ValidationResult.Success(),
                })
        );

        return threads ?? -1;
    }
}

internal sealed class UserPromptDirectory : IUserPrompt<string>
{
    public string Prompt()
    {
        string directoryPath = AnsiConsole.Prompt(
            new TextPrompt<string>("Выберите [green]директорию[/], которую желаете задействовать? (exit)")
                .PromptStyle("yellow")
                .Validate(path => {
                    if (string.IsNullOrWhiteSpace(path))
                        return ValidationResult.Error("[red]Путь не может быть пустым[/]");

                    if (path.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                        return ValidationResult.Success();


                    return Directory.Exists(path)
                        ? ValidationResult.Success() 
                        : ValidationResult.Error("[red]Ошибка:[/] такой директории не существует!");
                })
        );

        if (directoryPath.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine("[yellow]Операция отменена пользователем.[/]");
            return string.Empty;
        }

        return directoryPath;
    }
}

internal sealed class ExitConfirmationPrompt : IUserPrompt<bool>
{
    public bool Prompt() => AnsiConsole.Confirm("Выйти из приложения?");
}

internal sealed class UserPromptShowMoreLogs : IUserPrompt<bool>
{
    public bool Prompt() => AnsiConsole.Confirm("Показать другие логи?");
}
