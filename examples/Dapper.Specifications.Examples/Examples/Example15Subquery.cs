using Dapper.Specifications.Evaluators;
using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example15Subquery : IExample
{
    public int ExampleNumber => 15;
    public string Title => "Subquery in FROM Clause";
    public string Description => "Using subqueries as data sources";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var codePanel = new Panel(
            "[grey]// Subquery: Get average price per category[/]\n" +
            "var subquery = new ProductSpecification()\n" +
            "    [cyan].SetSelectClause[/]([green]\"category_id, AVG(price) as avg_price\"[/])\n" +
            "    [cyan].AddGroupBy[/]([green]\"category_id\"[/])\n\n" +
            "[grey]// Main query: Use subquery[/]\n" +
            "var spec = new ProductSpecification()\n" +
            "    [cyan].SetFromSubquery[/](subquery, [green]\"cat_avg\"[/])\n" +
            "    [cyan].AddWhere[/]([green]\"avg_price > @MinAvg\"[/], ...);"
        )
        {
            Header = new PanelHeader("[yellow]Subquery in FROM[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey)
        };

        AnsiConsole.Write(codePanel);
        AnsiConsole.WriteLine();

        // Subquery: Get average price per category
        var subquery = new ProductSpecification()
            .SetSelectClause("category_id AS CategoryId, AVG(price) as AvgPrice")
            .AddGroupBy("category_id");

        // Main query: Use subquery
        var spec = new ProductSpecification()
            .SetSelectClause("CategoryId, AvgPrice")
            .SetFromSubquery(subquery, "cat_avg")
            .AddWhere("AvgPrice > @MinAvg", new { MinAvg = 50 })
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
            .AddColumn(new TableColumn("[yellow]Average Price[/]").RightAligned());

        foreach (var result in results)
        {
            var categoryId = (int)result.CategoryId;
            var categoryName = categoryNames.GetValueOrDefault(categoryId, "Unknown");
            var avgPrice = (decimal)(double)result.AvgPrice;
            table.AddRow(
                $"[cyan]{categoryName}[/]",
                $"[green]${avgPrice:F2}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Query: Subquery calculating average price per category[/]\n");
    }
}