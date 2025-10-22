using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example08Exists : IExample
{
    public int ExampleNumber => 8;
    public string Title => "EXISTS Query";
    public string Description => "Checking for record existence";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var priceChecks = new[] { 1000, 500, 100, 50, 10 };

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[yellow]Price Threshold[/]")
            .AddColumn(new TableColumn("[yellow]Products Exist?[/]").Centered());

        foreach (var price in priceChecks)
        {
            var spec = new ProductSpecification()
                .AddWhere("price > @Price", new { Price = price });

            var exists = await connection.ExistsBySpecAsync(spec, SqlDialect.Sqlite);
            var statusIcon = exists ? "[green]✓ Yes[/]" : "[red]✗ No[/]";
            table.AddRow($"[cyan]> ${price}[/]", statusIcon);
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Query: SELECT EXISTS(SELECT 1 FROM products WHERE price > @Price)[/]\n");
    }
}