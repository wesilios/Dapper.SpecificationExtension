using Dapper.Specifications.Evaluators;
using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example06CustomSelect : IExample
{
    public int ExampleNumber => 6;
    public string Title => "Custom SELECT with Calculated Fields";
    public string Description => "Dynamic field calculations";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var spec = new ProductSpecification()
            .SetSelectClause("name AS Name, price AS Price, (price * 0.9) as DiscountedPrice")
            .AddWhere("price > @MinPrice", new { MinPrice = 50 });

        var results = await connection.QueryAsync(
            SpecificationEvaluator.Build(spec, SqlDialect.Sqlite).Sql,
            spec.Parameters
        );

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[yellow]Product Name[/]")
            .AddColumn(new TableColumn("[yellow]Original Price[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]Discounted (10%)[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]Savings[/]").RightAligned());

        foreach (var result in results)
        {
            string name = result.Name;
            double price = result.Price;
            double discountedPrice = result.DiscountedPrice;
            var savings = price - discountedPrice;
            table.AddRow(
                name,
                $"[dim]${price:F2}[/]",
                $"[green]${discountedPrice:F2}[/]",
                $"[red]-${savings:F2}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine(
            "[dim]Query: SELECT name, price, (price * 0.9) as DiscountedPrice WHERE price > $50[/]\n");
    }
}