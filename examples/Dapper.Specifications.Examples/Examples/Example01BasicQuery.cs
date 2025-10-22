using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example01BasicQuery : IExample
{
    public int ExampleNumber => 1;
    public string Title => "Basic Query with WHERE clause";
    public string Description => "Simple filtering with parameters";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var spec = new ProductSpecification()
            .SetSelectClause(
                "product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
            .AddWhere("price > @MinPrice", new { MinPrice = 50 });

        var products = await connection.QueryBySpecAsync(spec, SqlDialect.Sqlite);

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
        AnsiConsole.MarkupLine("[dim]Query: WHERE price > $50[/]\n");
    }
}