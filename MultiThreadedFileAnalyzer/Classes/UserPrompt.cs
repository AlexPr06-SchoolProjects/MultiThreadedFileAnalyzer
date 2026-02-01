using MultiThreadedFileAnalyzer.Interfaces;
using Spectre.Console;
using System.IO;

namespace MultiThreadedFileAnalyzer.Classes;
internal abstract class UserPromptInt : IPromptable<int>
{
    abstract public int Prompt();
}

internal abstract class UserPromptString : IPromptable<string>
{
    abstract public string Prompt();
}

internal abstract class UserPromptBool : IPromptable<bool>
{
    public abstract bool Prompt();
}

internal class UserPromptThreads : UserPromptInt
{
    public override int Prompt()
    {
        int? threads = AnsiConsole.Prompt(
            new TextPrompt<int?>("Сколько [green]потоков[/] задействовать?")
                .PromptStyle("yellow")
                .ValidationErrorMessage("[red]Ошибка:[/] введите число от 1 до 16")
                .Validate(value => value switch
                {
                    null => ValidationResult.Success(),
                    < 1 => ValidationResult.Error("Слишком мало! Нужно хотя бы 1"),
                    > 16 => ValidationResult.Error("Слишком много! Максимум 16"),
                    _ => ValidationResult.Success(),
                })
        );

        return threads ?? -1;
    }
}

internal class UserPromptDirectory : UserPromptString
{
    public override string Prompt()
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
            return String.Empty;
        }

        return directoryPath;
    }
}

internal class UserPromptBoolInApp : UserPromptBool
{
    public override bool Prompt()
    {
        var confirmed = AnsiConsole.Confirm("Выйти из приложения?");
        if (confirmed)
            return true;
        else
            return false;
    }
}

internal class UserPromptShowMoreLogs : UserPromptBool
{
    public override bool Prompt()
    {
        var confirmed = AnsiConsole.Confirm("Показать другие логи?");
        if (confirmed)
            return true;
        else
            return false;
    }
}
