using Microsoft.Extensions.DependencyInjection;
using MultiThreadedFileAnalyzer.Classes.Logs;
using MultiThreadedFileAnalyzer.Classes.Menu;
using MultiThreadedFileAnalyzer.Classes.Menu.MenuOptions;
using MultiThreadedFileAnalyzer.Interfaces;

namespace MultiThreadedFileAnalyzer.Classes.App;

internal sealed class App(
         AppLoopConditions appConditions,
         IMenuOptionFactory optionFactory,
         AppCleaner appCleaner,
         AppLayout appLayout,
         [FromKeyedServices("success")] LogPool successPool,
         [FromKeyedServices("failed")] LogPool failedPool,
         [FromKeyedServices("service")] LogPool servicePool,
         [FromKeyedServices("all")] LogPool allLogs,
         [FromKeyedServices("ExitConfirmationPrompt")] IUserPrompt<bool> promptInApp,
         [FromKeyedServices("UserPromptShowMoreLogs")]IUserPrompt<bool> promptShowMoreLogs
   )
{
    private readonly List<MenuOption> _menuOptions =
        [
            optionFactory.CreateWorkOption("Обработать txt файлы"),
            optionFactory.CreateClearOption("Очистить консоль")
        ];

    private readonly MenuClass _menu =  new MenuClass( [
        optionFactory.CreateWorkOption("Обработать txt файлы"),
        optionFactory.CreateClearOption("Очистить консоль")
    ]);

    private void Init()
    {
        appCleaner.AddSome(new List<ICleanable>()
        { 
            successPool, failedPool, servicePool, allLogs
        });
    }

    public void SetAppConditions(List<ICondition> conditions) => appConditions.AddSome(conditions);

    public void Run()
    {
        Init();
        appLayout.UpdateMenu(_menu.OwnRender());
        appLayout.Refresh();
        bool exitApp = false;

        while (appConditions.AreApplied() && !exitApp)
        {
            int userOption = _menu.AskForMenuOption(this._menuOptions.Count) - 1;
            var selectedOption = this._menuOptions[userOption];

            selectedOption.Execute();
            HandlePostExecute(selectedOption);
            if (promptInApp.Prompt()) break;
            appCleaner.CleanAll();
        }
    }

    private void HandlePostExecute(MenuOption selectedOption)
    {
        if (selectedOption is MenuOptionWork)
        {
            if (selectedOption.NeedsLogRendering)
            {
                appLayout.RenderLogs(servicePool, "[bold yellow] Логи (возможно не все влезли) [/]");
            }
               
            if (promptShowMoreLogs.Prompt())
            {
                RenderFullLogs();
            }
        }
        else if (selectedOption is ClearConsoleOption)
        {
            appLayout.Refresh();
        }
    }

    private void RenderFullLogs()
    {
        var logs = new List<LogPool> { successPool, failedPool, servicePool, allLogs };
        var names = new List<string> { "✅ УСПЕШНО", "❌ ОШИБКИ", "⚙ СЕРВИСНЫЕ", "📝 ВСЕ ЛОГИ" };
        List<int> logsColumns = new List<int>() { 1, 1, 1, 1 };
        appLayout.RenderAllLogsIntoConsole(logs, logsColumns, names);
    }
}

