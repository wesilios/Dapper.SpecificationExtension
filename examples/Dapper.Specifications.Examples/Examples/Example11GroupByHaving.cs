using Dapper.Specifications.Evaluators;
using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example11GroupByHaving : IExample
{
    public int ExampleNumber => 11;
    public string Title => "GROUP BY and HAVING";
    public string Description => "Aggregate functions with filtering";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var codePanel = new Panel(
            "[grey]var spec = new ProductSpecification()[/]\n" +
            "    [cyan].SetSelectClause[/]([green]\"category_id, COUNT(*) as ProductCount, AVG(price) as AvgPrice\"[/])\n" +
            "    [cyan].AddGroupBy[/]([green]\"category_id\"[/])\n" +
            "    [cyan].AddHaving[/]([green]\"COUNT(*) > @MinCount\"[/], ...)\n" +
            "    [cyan].AddOrder[/]([green]\"AvgPrice DESC\"[/]);"
        )
        {
            Header = new PanelHeader("[yellow]GROUP BY with HAVING[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey)
        };

        AnsiConsole.Write(codePanel);
        AnsiConsole.WriteLine();

        var spec = new ProductSpecification()
            .SetSelectClause(
                "category_id AS CategoryId, COUNT(*) as ProductCount, AVG(price) as AvgPrice, MIN(price) as MinPrice, MAX(price) as MaxPrice")
            .AddGroupBy("category_id")
            .AddHaving("COUNT(*) > @MinCount", new { MinCount = 1 })
            .AddOrder("AvgPrice DESC");

        var results = await connection.QueryAsync(
            SpecificationEvaluator.Build(spec, SqlDialect.Sqlite).Sql,
            spec.Parameters
        );

        var categoryNames = new Dictionary<int, string>
        {
            { 1, "Electronics" },
            { 2, "Books" },
            { 3, "Clothing" }
        };

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[yellow]Category[/]")
            .AddColumn(new TableColumn("[yellow]Count[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]Avg Price[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]Min Price[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]Max Price[/]").RightAligned());

        foreach (var result in results)
        {
            var categoryId = (int)result.CategoryId;
            var categoryName = categoryNames.GetValueOrDefault(categoryId, "Unknown");
            table.AddRow(
                $"[cyan]{categoryName}[/]",
                $"[green]{result.ProductCount}[/]",
                $"[yellow]${result.AvgPrice:F2}[/]",
                $"[dim]${result.MinPrice:F2}[/]",
                $"[magenta]${result.MaxPrice:F2}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Query: GROUP BY category_id with aggregate functions and HAVING clause[/]\n");
    }
}