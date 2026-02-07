using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultiThreadedFileAnalyzer.Classes.App;
using MultiThreadedFileAnalyzer.Classes.Factories;
using MultiThreadedFileAnalyzer.Classes.FileProcessor;
using MultiThreadedFileAnalyzer.Classes.Logs;
using MultiThreadedFileAnalyzer.Classes.Menu;
using MultiThreadedFileAnalyzer.Interfaces;


ThreadPool.SetMinThreads(20, 2);
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

HostApplicationBuilder builder = new HostApplicationBuilder(args);

// P.S Можно было реализовать логику добавления сервисов каждым логическим
// блоком программы ( ниже: App, Prompts, Logs и т.д.) самостоятельно или предоставляния каждым из сервисов 
// некую для єтого абстракцию реализуюющую подобного плана идею.
// Однако я нахожу єтот подход излишним для такого маленького проекта.


// --------------------------------------- BUILDER CONFIGURATION ---------------------------------------

// App
builder.Services.AddSingleton<AppLoopConditions>();
builder.Services.AddSingleton<AppCleaner>();
builder.Services.AddSingleton<AppLayout>();

// Prompts
builder.Services.AddKeyedSingleton<IUserPrompt<string>, UserPromptDirectory>("UserPromptDirectory");
builder.Services.AddKeyedSingleton<IUserPrompt<int>, UserPromptThreads>("UserPromptThreads");
builder.Services.AddKeyedSingleton<IUserPrompt<bool>, ExitConfirmationPrompt>("ExitConfirmationPrompt");
builder.Services.AddKeyedSingleton<IUserPrompt<bool>, UserPromptShowMoreLogs>("UserPromptShowMoreLogs");

// Logs
builder.Services.AddKeyedSingleton<LogPool>("success");
builder.Services.AddKeyedSingleton<LogPool>("failed");
builder.Services.AddKeyedSingleton<LogPool>("service");
builder.Services.AddKeyedSingleton<LogPool>("all");

// Other
builder.Services.AddKeyedSingleton<IFileStatisticsManager, FileStatisticsManager>("FileStatisticsManager");

builder.Services.AddSingleton<IMenuOptionFactory, MenuOptionFactory>();

builder.Services.AddSingleton<IOwnRenderable, MenuClass>();

builder.Services.AddKeyedSingleton<IFileProcessorClass, FileProcessorClass>("FileProcessorClass", (sp, key) =>
{
    return new FileProcessorClass(
            sp.GetRequiredKeyedService<LogPool>("success"),
            sp.GetRequiredKeyedService<LogPool>("failed"),
            sp.GetRequiredKeyedService<LogPool>("service"),
            sp.GetRequiredKeyedService<LogPool>("all")
        );
});

builder.Services.AddSingleton<App>();

// --------------------------------------- BUILDER CONFIGURATION ---------------------------------------


using IHost host = builder.Build();

App app = host.Services.GetRequiredService<App>();
app.Run();
