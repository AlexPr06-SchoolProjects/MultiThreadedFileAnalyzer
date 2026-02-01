using MultiThreadedFileAnalyzer.Classes.Logs;
using Spectre.Console;
using Spectre.Console.Rendering;

using MenuClass = MultiThreadedFileAnalyzer.Classes.Menu.Menu;

namespace MultiThreadedFileAnalyzer.Classes.App
{

    internal class AppLayout
    {
        private Layout _rootLayout;
        private Layout _rootUpperLayout;
        private Layout _rootBottomLayout;
        private Layout _leftLayout;
        public readonly Layout _rightLayout;
        public readonly Layout _leftUpperLayout;
        public readonly Layout _leftBottomLayout;

        private string _rootLayoutName;
        private string _rootUpperLayoutName;
        private string _rootBottomLayoutName;
        private string _leftLayoutName;
        private string _rightLayoutName;
        private string _leftUpperLayoutName;
        private string _leftBottomLayoutName;


        public AppLayout()
        {
            _rootLayoutName = "Root";
            _rootUpperLayoutName = "Upper";
            _rootBottomLayoutName = "Bottom";
            _leftLayoutName = "Left";
            _rightLayoutName = "Right";
            _leftUpperLayoutName = "Left_Upper";
            _leftBottomLayoutName = "Left_Bottom";

            _rootLayout = new Layout(_rootLayoutName);
            _rootUpperLayout = new Layout(_rootUpperLayoutName);
            _rootBottomLayout = new Layout(_rootBottomLayoutName);
            _leftLayout = new Layout(_leftLayoutName);
            _rightLayout = new Layout(_rightLayoutName);
            _leftUpperLayout = new Layout(_leftUpperLayoutName);
            _leftBottomLayout = new Layout(_leftBottomLayoutName);
        }

        public void UpdateMenu(MenuClass menu)
        {
            _leftUpperLayout.Update(menu.OwnRender());
            RefreshLayout();
        }
        public void RefreshLayout()
        {
            AnsiConsole.Live(_rootLayout).Start(ctx =>
            {
                ctx.Refresh();
            });
        }

        public void RenderLayout()
        {
            _rootLayout
                .SplitRows(
                    _rootUpperLayout.Size(5),
                    _rootBottomLayout
                        .SplitColumns(
                            _leftLayout.Size(50)
                                .SplitRows(
                                    _leftUpperLayout,
                                    _leftBottomLayout
                                ),
                            _rightLayout
                        )
                );

            _rootUpperLayout.Update(
                 new Panel(new Markup("[bold cyan1]=== МНОГОПОТОЧНЫЙ ПОДСЧЕТ ФАЙЛОВ ===[/]").Centered())
                    .Expand()
                    .BorderColor(Color.DarkGoldenrod)
                    .Padding(0, 1, 0, 1)
            );

            _leftUpperLayout.Update(
                new Panel(new Text("Меню"))
                    .Header("Меню")
                    .Expand()
                    .BorderColor(Color.DarkGoldenrod)
            );

            _leftBottomLayout.Update(
                new Panel(new Text("Результат"))
                    .Header("Здесь появится результат")
                    .Expand()
                    .BorderColor(Color.DarkKhaki)
            );

            _rightLayout["Right"].Update(
                new Panel(new Text("Логи"))
                    .Header("Логи")
                    .Expand()
                    .BorderColor(Color.DarkSeaGreen1_1)
            );
        }

        public void LogsRenderForLayout(LogPool logsPool, int colsAmount, string headerText)
        {
            var renderable = new List<IRenderable>();

            foreach (var log in logsPool._loggableItems) 
                if (log is not null)
                    renderable.Add(log.Log());
            if (renderable.Count == 0)
                _rightLayout.Update(new Panel("Логов пока нет...").Expand());
            else
            {
                Grid grid = CreateGrid(colsAmount, renderable);
                var panel = new Panel(grid)
                    .Header(headerText)
                    .Expand()
                    .BorderColor(Color.DarkSeaGreen1_1)
                    .RoundedBorder();

                _rightLayout.Update(panel);
            }

            RefreshLayout();
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
            AnsiConsole.Write(grid);
        }

        private Panel CreatePanelOfLogs(LogPool logsPool, int colsAmount, string headerText)
        {
            var renderables = new List<IRenderable>();

            foreach (var log in logsPool._loggableItems)
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
