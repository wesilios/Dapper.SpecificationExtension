using Dapper.Specifications.Evaluators;
using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example12Distinct : IExample
{
    public int ExampleNumber => 12;
    public string Title => "DISTINCT";
    public string Description => "Selecting unique values";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var codePanel = new Panel(
            "[grey]var spec = new ProductSpecification()[/]\n" +
            "    [cyan].SetDistinct[/]()\n" +
            "    [cyan].SetSelectClause[/]([green]\"category_id\"[/])\n" +
            "    [cyan].AddOrder[/]([green]\"category_id ASC\"[/]);"
        )
        {
            Header = new PanelHeader("[yellow]DISTINCT Query[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey)
        };

        AnsiConsole.Write(codePanel);
        AnsiConsole.WriteLine();

        var spec = new ProductSpecification()
            .SetDistinct()
            .SetSelectClause("category_id AS CategoryId")
            .AddOrder("category_id ASC");

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
            .AddColumn("[yellow]Unique Categories[/]");

        foreach (var result in results)
        {
            var categoryId = (int)result.CategoryId;
            var categoryName = categoryNames.GetValueOrDefault(categoryId, "Unknown");
            table.AddRow($"[cyan]{categoryName}[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Query: SELECT DISTINCT category_id FROM products[/]\n");
    }
}