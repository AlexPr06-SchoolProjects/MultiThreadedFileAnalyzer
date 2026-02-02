using MultiThreadedFileAnalyzer.Classes.App;

ThreadPool.SetMinThreads(20, 2);

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

AppLoopConditions appLoopConditions = new AppLoopConditions();

App app = new App(appLoopConditions);
app.Run();
