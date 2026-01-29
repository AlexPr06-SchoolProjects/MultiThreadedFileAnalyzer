using System;
using System.Threading;

namespace HomeworkApp
{
	class Program
	{
		static void Main()
		{
			// This is a comment inside a text file
			Console.WriteLine("Starting multi-threaded analysis...");

			for (int i = 0; i < 5; i++)
			{
				int threadNum = i;
				new Thread(() => ProcessData(threadNum)).Start();
			}
		}

		static void ProcessData(int id)
		{
			Console.WriteLine($"Worker {id} is busy.");
			Thread.Sleep(100);
		}
	}
}