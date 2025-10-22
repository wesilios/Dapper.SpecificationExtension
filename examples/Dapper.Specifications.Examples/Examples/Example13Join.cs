using Dapper.Specifications.Evaluators;
using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example13Join : IExample
{
    public int ExampleNumber => 13;
    public string Title => "INNER JOIN";
    public string Description => "Joining multiple tables";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var codePanel = new Panel(
            "[grey]var spec = new ProductJoinSpecification()[/]\n" +
            "    [cyan].SetSelectClause[/]([green]\"p.product_id, p.name, p.price, c.name as CategoryName\"[/])\n" +
            "    [cyan].AddInnerJoin[/]([green]\"categories c ON p.category_id = c.category_id\"[/])\n" +
            "    [cyan].AddWhere[/]([green]\"p.price > @MinPrice\"[/], ...)\n" +
            "    [cyan].AddOrder[/]([green]\"p.price DESC\"[/]);"
        )
        {
            Header = new PanelHeader("[yellow]INNER JOIN Query[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey)
        };

        AnsiConsole.Write(codePanel);
        AnsiConsole.WriteLine();

        // For JOINs, we use a custom specification with table alias
        var spec = new ProductJoinSpecification()
            .SetSelectClause("p.product_id AS ProductId, p.name AS Name, p.price AS Price, c.name AS CategoryName")
            .AddInnerJoin("categories c ON p.category_id = c.category_id")
            .AddWhere("p.price > @MinPrice", new { MinPrice = 50 })
            .AddOrder("p.price DESC");

        var results = await connection.QueryAsync(
            SpecificationEvaluator.Build(spec, SqlDialect.Sqlite).Sql,
            spec.Parameters
        );

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[yellow]Product Name[/]")
            .AddColumn("[yellow]Category[/]")
            .AddColumn(new TableColumn("[yellow]Price[/]").RightAligned());

        foreach (var result in results)
        {
            string name = result.Name;
            string categoryName = result.CategoryName;
            var price = (decimal)(double)result.Price;
            table.AddRow(
                name,
                $"[cyan]{categoryName}[/]",
                $"[green]${price:F2}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Query: INNER JOIN categories with products WHERE price > $50[/]\n");
    }
}