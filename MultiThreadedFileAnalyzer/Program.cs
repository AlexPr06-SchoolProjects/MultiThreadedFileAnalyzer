using MultiThreadedFileAnalyzer.Classes;
using MultiThreadedFileAnalyzer.Interfaces;
using System.Collections.Concurrent;

Console.WriteLine("=== МНОГОПОТОЧНЫЙ ПОДСЧЕТ ФАЙЛОВ ===");

FileStatisticsManager fsm = new FileStatisticsManager();

// User input ====================================================
string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles");
int numberOfThreads = 4;
// User input ====================================================

// Finding all files
string[]? fileNames = fsm.FindAllTxtFiles(directoryPath);
if (fileNames is null)
    Console.WriteLine($"No files founded in provided directory");


ConcurrentStack<FileTask>? stackOfFileTasks = fsm.PutFilesIntoStack(directoryPath, fileNames);
if (stackOfFileTasks is null)
    Console.WriteLine($"No files founded in provided directory");

foreach (var fileTask in stackOfFileTasks)
    Console.WriteLine($"{fileTask.Name}");

// Semaphore logic 
Semaphore semaphore = new Semaphore(0, numberOfThreads);

semaphore.Release(numberOfThreads);

// Processing all files
var fileStatisticsList =  ProcessFilesInParallel(
    semaphore, directoryPath, fsm, stackOfFileTasks, numberOfThreads
    );

foreach (var file in fileStatisticsList)
{
    Console.WriteLine($"{file.FileName}: Lines:{file.LinesCount} WordsCount: {file.WordsCount} CharactersCount: {file.CharactersCount}");
}

List<IFileStatistics> ProcessFilesInParallel(
    Semaphore semaphore, string directoryPath, FileStatisticsManager fsm, 
    ConcurrentStack<FileTask> fileNamesStack, int maxThreads)
{
    List<IFileStatistics> fileStatisticsList = new List<IFileStatistics>();
    object locker = new object();
    TimeSpan timeSpan = TimeSpan.FromSeconds(5);

    int threadsToStart = Math.Min(maxThreads, fileNamesStack.Count);

    using (CountdownEvent countdown = new CountdownEvent(threadsToStart))
    {
        for (int i = 0; i < threadsToStart; ++i)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                ProcessFile(
                    fileNamesStack, locker, fileStatisticsList,
                    fsm, semaphore, timeSpan, countdown);
            });
        }
        countdown.Wait();
    };

    return fileStatisticsList;
}

void ProcessFile(
    ConcurrentStack<FileTask> fileNamesStack, object locker, 
    List<IFileStatistics> fileStatisticsList,  FileStatisticsManager 
    fsm, Semaphore semaphore, TimeSpan timeSpan, CountdownEvent countdown)
{
    try
    {
        while (fileNamesStack.TryPop(out FileTask? task))
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            if (semaphore.WaitOne(timeSpan))
            {
                Console.WriteLine($"{threadId} начал работу.");

                try
                {
                    string filePath = Path.Combine(directoryPath, task.Name);
                    IFileStatistics fileStatistics = fsm.Analize(filePath, task.Name);
                    lock (locker)
                    {
                        fileStatisticsList.Add(fileStatistics);
                    }
                    
                }
                catch (Exception ex)
                {
                    task.RetryCount++;
                    if (task.RetryCount < 3)
                        fileNamesStack.Push(task);
                    Console.WriteLine($"ERROR in {threadId}: {ex.Message}. Попытка: {task.RetryCount}/3");
                    Console.WriteLine($"{task.Name} был положен в стек.");
                }
                finally
                {

                    Console.WriteLine($"{threadId} закончил работу.");
                    semaphore.Release();
                }
            }
            else
            {
                Console.WriteLine($"{threadId} не дождался своей очереди после 3 попыток.");
            }
        }
    }
    finally
    {
        countdown.Signal();
    }
}
