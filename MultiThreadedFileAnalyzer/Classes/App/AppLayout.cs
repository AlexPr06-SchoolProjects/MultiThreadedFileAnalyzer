using MultiThreadedFileAnalyzer.Classes.FileProcessor;
using MultiThreadedFileAnalyzer.Classes.Logs;
using MultiThreadedFileAnalyzer.Constants;
using MultiThreadedFileAnalyzer.Interfaces;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Collections.Concurrent;

using MultiThreadedFileAnalyzer.Classes.Builders;

namespace MultiThreadedFileAnalyzer.Classes.App
{

    internal sealed class AppLayout
    {
        private readonly Layout _root;
        private readonly Layout _menuZone;
        private readonly Layout _resultsZone;
        private readonly Layout _logsZone;

        public AppLayout()
        {
            var (root, menu, results, logs) = LayoutBuilder.Build();
            _root = root;
            _menuZone = menu;
            _resultsZone = results;
            _logsZone = logs;
        }

        public void RunLive(Action<LiveDisplayContext> action) => AnsiConsole.Live(_root).AutoClear(false).Start(action);
        
        public void UpdateMenu(IRenderable content) => _menuZone.Update(content);
        public void UpdateResults(IRenderable content) => _resultsZone.Update(content);
        public void UpdateLogs(IRenderable content) => _logsZone.Update(content);

        public void Refresh() => AnsiConsole.Write(_root);

        public void RenderLogs(LogPool logsPool, string headerText)
        {
            List<IRenderable> logItems = logsPool.AllLogs.Select(l => l.Log()).ToList();

            if (!logItems.Any()) return;

            var grid = new Grid().AddColumn();
            foreach (var log in logItems) grid.AddRow(log);

            var panel = new Panel(grid)
                .Header(headerText)
                .Expand()
                .RoundedBorder()
                .BorderColor(Color.DarkSeaGreen1_1);

            UpdateLogs(panel);
        }

        public void RenderResultTable(Table table, string panelHeader)
        {
            var panel = new Panel(table)
                    .Header(panelHeader)
                    .Expand()
                    .RoundedBorder()
                    .BorderColor(AppColors.TreePanel);

            _resultsZone.Update(panel);
        }

        public void LogsRenderIntoConsole(LogPool logsPool, int colsAmount, string headerText)
        {
            Panel panel = CreatePanelOfLogs(logsPool, colsAmount, headerText);
            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }

        public void RenderAllLogsIntoConsole(List<LogPool> logsPools, List<int> colsAmounts, List<string> headerTexts)
        {
            object locker = new object();
            List<Panel> panelsToRender = new List<Panel>();
            List<Task> tasks = new List<Task>();

            Parallel.ForEach(logsPools, logs =>
            {

            });

            for (int i = 0; i < logsPools.Count; i++)
            {
                var currentPool = logsPools[i];
                var currentCols = colsAmounts[i];
                var currentHeader = headerTexts[i];

                Task t = new Task(() =>
                {
                    Panel p = CreatePanelOfLogs(currentPool, currentCols, currentHeader);
                    lock (locker) {
                        panelsToRender.Add(p);
                    }
                });

                tasks.Add(t);
                t.Start();
            }

            Task.WaitAll(tasks);

            var grid = new Grid();
            for (int i = 0; i < panelsToRender.Count; i++)
                grid.AddColumn(new GridColumn());

            grid.AddRow(panelsToRender.ToArray());
            AnsiConsole.WriteLine();
            AnsiConsole.Write(grid);
            AnsiConsole.WriteLine();
        }

        private Panel CreatePanelOfLogs(LogPool logsPool, int colsAmount, string headerText)
        {
            var renderables = new List<IRenderable>();

            foreach (var log in logsPool.AllLogs)
                if (log is not null)
                    renderables.Add(log.Log());
            if (renderables.Count == 0)
                return new Panel("[grey]Логов пока нет...[/]")
                    .Header(headerText)
                    .BorderColor(Color.DarkSeaGreen1_1)
                    .RoundedBorder();


            Grid grid = CreateGrid(colsAmount, renderables);

            var panel = new Panel(grid)
                .Header(headerText)
                .BorderColor(Color.DarkSeaGreen1_1)
                .RoundedBorder();

            return panel;
        }

        private Grid CreateGrid(int colsAmount, List<IRenderable> renderables)
        {
            var grid = new Grid();

            for (int n = 0; n < colsAmount; n++)
            {
                grid.AddColumn(new GridColumn().NoWrap());
            }

            for (int i = 0; i < renderables.Count; i += colsAmount)
            {
                var row = renderables.Skip(i).Take(colsAmount).ToList();
                while (row.Count < colsAmount)
                    row.Add(new Text(string.Empty));
                grid.AddRow(row.ToArray());
            }

            return grid;
        }

        public void ShowCurrentDirectory()
        {
            var path = Directory.GetCurrentDirectory();

            var pathText = new TextPath(path)
                 .RootColor(Color.Chartreuse2_1)
                .SeparatorColor(Color.Grey)
                .StemColor(Color.Cornsilk1)
                .LeafColor(Color.Chartreuse3);
       

            var panel = new Panel(pathText)
            {
                Header = new PanelHeader("📂 Текущая рабочая область"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Yellow),
                Padding = new Padding(1, 0, 1, 0),
                Expand = false
            };

            AnsiConsole.Write(panel);
        }

        public Tree GetTreeFromFiles(string rootDirectory, IProducerConsumerCollection<FileTask> fileTasks)
        {
            var tree = new Tree($"[yellow]{Markup.Escape(rootDirectory)}[/]");
            var nodes = new Dictionary<string, TreeNode>();

            foreach (var task in fileTasks)
            {
                string relativePath = task.Name;
                string[] pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                IHasTreeNodes currentNode = tree;
                string accumulatedPath = String.Empty;

                for (int i = 0; i < pathParts.Length; ++i) 
                {
                    string part = pathParts[i];

                    accumulatedPath = string.IsNullOrEmpty(accumulatedPath) ? part : Path.Combine(accumulatedPath, part);
                    bool isFile = (i == pathParts.Length - 1);

                    if (nodes.TryGetValue(accumulatedPath, out var existingNode))
                        currentNode = existingNode;
                    else
                    {
                        Color color = isFile ? AppColors.File : AppColors.Folder;
                        string colorName = color.ToMarkup();
                        string icon = isFile ? ":page_facing_up:" : ":open_file_folder:";

                        TreeNode newNode = currentNode.AddNode($"{icon} [{colorName}]{Markup.Escape(part)}[/]");

                        nodes[accumulatedPath] = newNode;
                        currentNode = newNode;
                    }
                }
            }

            return tree;
        }

        public void RenderTree(Tree tree, string panelHeader)
        {
            Panel panel = new Panel(tree)
                .Header(panelHeader)
                .RoundedBorder()
                .BorderColor(AppColors.TreePanel);

            AnsiConsole.WriteLine();
            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }

        public Table CreateResultTable(IEnumerable<IFileStatistics> fileTasks)
        {
            Table table = new Table();

            table.Border(TableBorder.Rounded);
            table.Title("[yellow]РЕЗУЛЬТАТ АНАЛИЗА ФАЙЛОВ[/]");
            table.ShowRowSeparators();

            table.AddColumn(new TableColumn("Path").Width(50).LeftAligned());
            table.AddColumn(new TableColumn("[blue]Lines[/]").Centered());
            table.AddColumn(new TableColumn("[blue]Words[/]").Centered());
            table.AddColumn(new TableColumn("[blue]Chars[/]").Centered());

            foreach(var task in fileTasks)
            {
                table.AddRow(
                    Markup.Escape(task.FileName),               
                    $"[green]{task.LinesCount}[/]",          
                    $"[cyan]{task.WordsCount}[/]",           
                    $"[magenta]{task.CharactersCount}[/]"    
                );
            }

            return table;
        }

        public void ShowErrorMessage(string message) => UIHelper.ShowErrorMessage(message);

        public void ShowSuccessMessage(string message) => UIHelper.ShowSuccessMessage(message);

        public static void ShowWarningMessage(string message) => UIHelper.ShowWarningMessage(message);
    }
}


public static class UIHelper
{
    public static void ShowErrorMessage(string message)
    {
        AnsiConsole.Write(
            new Panel(new Markup($"[white]{Markup.Escape(message)}[/]"))
                .Header("[bold red] ERROR [/]")
                .BorderColor(Color.Red)
                .RoundedBorder()
                .Expand() 
        );
    }

    public static void ShowSuccessMessage(string message)
    {
        AnsiConsole.MarkupLine($"[bold green]✅ Success:[/] [white]{Markup.Escape(message)}[/]");
    }

    public static void ShowWarningMessage(string message)
    {
        var rule = new Rule($"[yellow]⚠ {Markup.Escape(message)}[/]");
        rule.Justification = Justify.Left; 
        rule.Style = Style.Parse("yellow");

        AnsiConsole.Write(rule);
    }
}
