using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example03WhereOr : IExample
{
    public int ExampleNumber => 3;
    public string Title => "OR conditions";
    public string Description => "Using AddWhereOr() for alternative conditions";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var spec = new ProductSpecification()
            .SetSelectClause(
                "product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
            .AddWhere("price > @HighPrice", new { HighPrice = 500 })
            .AddWhereOr("featured = @Featured", new { Featured = 1 });

        var products = await connection.QueryBySpecAsync(spec, SqlDialect.Sqlite);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[yellow]Product Name[/]")
            .AddColumn(new TableColumn("[yellow]Price[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]Featured[/]").Centered());

        foreach (var product in products)
        {
            var featuredIcon = product.Featured ? "[green]★[/]" : "[dim]☆[/]";
            table.AddRow(product.Name, $"[green]${product.Price:F2}[/]", featuredIcon);
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Query: WHERE price > $500 OR featured = 1[/]\n");
    }
}