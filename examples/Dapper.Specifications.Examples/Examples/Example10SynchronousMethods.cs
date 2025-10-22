using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example10SynchronousMethods : IExample
{
    public int ExampleNumber => 10;
    public string Title => "Synchronous Methods";
    public string Description => "Non-async alternatives";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var spec = new ProductSpecification()
            .SetSelectClause(
                "product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
            .AddWhere("category_id = @CategoryId", new { CategoryId = 2 });

        // Synchronous query
        var products = connection.QueryBySpec(spec, SqlDialect.Sqlite);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[yellow]Product Name[/]")
            .AddColumn(new TableColumn("[yellow]Price[/]").RightAligned());

        foreach (var product in products)
        {
            table.AddRow(product.Name, $"[green]${product.Price:F2}[/]");
        }

        AnsiConsole.Write(table);

        // Synchronous count and exists
        var count = connection.CountBySpec(spec, SqlDialect.Sqlite);
        var exists = connection.ExistsBySpec(spec, SqlDialect.Sqlite);

        var infoPanel = new Panel(
            $"[cyan]Total Count:[/] [green]{count}[/]\n" +
            $"[cyan]Exists:[/] [green]{(exists ? "✓ Yes" : "✗ No")}[/]"
        )
        {
            Header = new PanelHeader("[yellow]Query Statistics[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey)
        };

        AnsiConsole.Write(infoPanel);
        AnsiConsole.MarkupLine("\n[dim]Using synchronous methods: QueryBySpec(), CountBySpec(), ExistsBySpec()[/]\n");

        await Task.CompletedTask;
    }
}