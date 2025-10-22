using Dapper.Specifications.Evaluators;
using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example14Union : IExample
{
    public int ExampleNumber => 14;
    public string Title => "UNION";
    public string Description => "Combining results from multiple queries";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var codePanel = new Panel(
            "[grey]// Expensive products[/]\n" +
            "var spec1 = new ProductSpecification()\n" +
            "    [cyan].SetSelectClause[/]([green]\"name, price, 'Expensive' as Category\"[/])\n" +
            "    [cyan].AddWhere[/]([green]\"price > @HighPrice\"[/], ...)\n\n" +
            "[grey]// Featured products[/]\n" +
            "var spec2 = new ProductSpecification()\n" +
            "    [cyan].SetSelectClause[/]([green]\"name, price, 'Featured' as Category\"[/])\n" +
            "    [cyan].AddWhere[/]([green]\"featured = @Featured\"[/], ...)\n\n" +
            "spec1[cyan].AddUnion[/](spec2);"
        )
        {
            Header = new PanelHeader("[yellow]UNION Query[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey)
        };

        AnsiConsole.Write(codePanel);
        AnsiConsole.WriteLine();

        // Expensive products
        var spec1 = new ProductSpecification()
            .SetSelectClause("name AS Name, price AS Price, 'Expensive' AS Category")
            .AddWhere("price > @HighPrice", new { HighPrice = 500 });

        // Featured products
        var spec2 = new ProductSpecification()
            .SetSelectClause("name AS Name, price AS Price, 'Featured' AS Category")
            .AddWhere("featured = @Featured", new { Featured = 1 });

        spec1.AddUnion(spec2);

        // Build the query and merge parameters from both specs
        var (sql, parameters) = SpecificationEvaluator.Build(spec1, SqlDialect.Sqlite);

        var results = await connection.QueryAsync(
            sql,
            parameters
        );

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[yellow]Product Name[/]")
            .AddColumn(new TableColumn("[yellow]Price[/]").RightAligned())
            .AddColumn("[yellow]Category[/]");

        foreach (var result in results)
        {
            string name = result.Name;
            var price = (decimal)(double)result.Price;
            string category = result.Category;
            var categoryColor = category == "Expensive" ? "red" : "green";
            table.AddRow(
                name,
                $"[green]${price:F2}[/]",
                $"[{categoryColor}]{category}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Query: UNION of expensive products and featured products[/]\n");
    }
}