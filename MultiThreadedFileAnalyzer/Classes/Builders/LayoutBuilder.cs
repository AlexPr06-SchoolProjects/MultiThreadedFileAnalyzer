using Spectre.Console;

namespace MultiThreadedFileAnalyzer.Classes.Builders;

internal static class LayoutBuilder
{
    public static (Layout Root, Layout Menu, Layout Results, Layout Logs) Build()
    {
        var menu = new Layout("Menu");
        var results = new Layout("Results");
        var logs = new Layout("Logs");
        var header = new Layout("Header").Size(5);

        var root = new Layout("Root")
            .SplitRows(
                header,
                new Layout("Main").SplitColumns(
                    new Layout("Left").Size(50).SplitRows(menu, results),
                    logs
                )
            );

        header.Update(CreateHeader());
        menu.Update(CreateDefaultPanel("Меню", "Загрузка...", Color.DarkGoldenrod));
        results.Update(CreateDefaultPanel("Результат", "Ожидание анализа...", Color.DarkKhaki));
        logs.Update(CreateDefaultPanel("Логи", "Активность отсутствует", Color.DarkSeaGreen1_1));

        return (root, menu, results, logs);
    }

    private static Panel CreateHeader() =>
        new Panel(new Markup("[bold cyan1]=== МНОГОПОТОЧНЫЙ АНАЛИЗАТОР ФАЙЛОВ ===[/]").Centered())
            .Expand().BorderColor(Color.DarkGoldenrod).Padding(0, 1, 0, 1);

    private static Panel CreateDefaultPanel(string header, string text, Color color) =>
        new Panel(new Text(text)).Header(header).Expand().BorderColor(color);
}
