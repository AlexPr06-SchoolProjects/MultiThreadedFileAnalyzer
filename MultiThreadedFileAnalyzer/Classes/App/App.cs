using MultiThreadedFileAnalyzer.Classes.FileProcessor;
using MultiThreadedFileAnalyzer.Classes.Logs;
using MultiThreadedFileAnalyzer.Classes.Menu;
using MultiThreadedFileAnalyzer.Interfaces;
using System.Collections.Concurrent;

using FileProcessorClass = MultiThreadedFileAnalyzer.Classes.FileProcessor.FileProcessor;
using MenuClass = MultiThreadedFileAnalyzer.Classes.Menu.Menu;

namespace MultiThreadedFileAnalyzer.Classes.App;

internal class App
{
    private AppCleaner _appCleaner;
    private AppLayout _appLayout;
    private AppLoopConditions _appConditions;
    private MenuClass _menu;
    private List<MenuOption> _menuOptions;

    private UserPromptThreads _userPromptThreads;
    private UserPromptDirectory _userPromptDirectory;
    private UserPromptBoolInApp _userPromptInApp;
    private UserPromptShowMoreLogs _userPromptShowMoreLogs;

    private FileStatisticsManager _fsm;

    private LogPool _failedItemsPool;
    private LogPool _successfulItemsPool;
    private LogPool _serviceItemsPool;
    private LogPool _allLogs;

    private FileProcessorClass _fileProcessor;

    public App(AppLoopConditions appConditions)
    {
        _appCleaner = new AppCleaner();
        _appLayout = new AppLayout();
        _appConditions = appConditions;
        _menu = new MenuClass();
        _menuOptions = new List<MenuOption>();
        _menuOptions.Add(new MenuOptionWork("Начать работу"));
        _menuOptions.Add(new MenuOptionWork("Очистить"));
        _menu.Init(_menuOptions);

        _userPromptThreads = new UserPromptThreads();
        _userPromptDirectory = new UserPromptDirectory();
        _userPromptInApp = new UserPromptBoolInApp();
        _userPromptShowMoreLogs = new UserPromptShowMoreLogs();

        _fsm = new FileStatisticsManager();

        _failedItemsPool = new LogPool();
        _successfulItemsPool = new LogPool();
        _serviceItemsPool = new LogPool();
        _allLogs = new LogPool();

        _fileProcessor = new FileProcessorClass(_failedItemsPool, _successfulItemsPool, _serviceItemsPool, _allLogs, string.Empty);



        _appCleaner.AddSome(new List<ICleanable>()
        {
            _failedItemsPool, _successfulItemsPool, _serviceItemsPool, _allLogs
        });
    }

    public void SetAppConditions(List<ICondition> conditions) => _appConditions.AddSome(conditions);


    public void Run()
    {
        _appLayout.RenderLayout();
        _appLayout.UpdateMenu(_menu);
        bool exitApp = false;
        while (_appConditions.AreApplied() && !exitApp)
        {
            int userOption = _menu.AskForMenuOption(_menuOptions.Count);
            var selectedOption = _menuOptions[userOption - 1];
            switch (userOption)
            {
                case 1:
                    if (selectedOption is MenuOptionWork workOption)
                    {
                        MenuOptionWorkParams menuOptionWorkParams = new MenuOptionWorkParams(
                            _appLayout, _userPromptDirectory, _userPromptThreads, _fileProcessor, _fsm);
                        workOption.AddParams(menuOptionWorkParams);
                    }    
                    break;
            }
            selectedOption.Execute();
            _appLayout.LogsRenderForLayout(_serviceItemsPool, 3, "[bold yellow] Последние логи [/]");

            bool showMoreLogs = _userPromptShowMoreLogs.Prompt();
            if (showMoreLogs) {
                List<LogPool> logs = new List<LogPool>() { _successfulItemsPool, _failedItemsPool, _serviceItemsPool, _allLogs  };
                List<int> logsColumns = new List<int>() { 1, 1, 1, 1 };
                List<string> logsColumnNames = new List<string>(){ "✅ УСПЕШНО", "❌ ОШИБКИ", "⚙ СЕРВИСНЫЕ",  "📝 ВСЕ ЛОГИ" };
                _appLayout.RenderAllLogsIntoConsole(logs, logsColumns, logsColumnNames);
            }

            _appCleaner.CleanAll();

            exitApp = _userPromptInApp.Prompt();
        }
    }
}

