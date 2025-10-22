using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example02MultipleWhereAnd : IExample
{
    public int ExampleNumber => 2;
    public string Title => "Multiple WHERE conditions (AND)";
    public string Description => "Combining multiple filters";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var spec = new ProductSpecification()
            .SetSelectClause(
                "product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
            .AddWhere("price > @MinPrice", new { MinPrice = 30 })
            .AddWhere("stock > @MinStock", new { MinStock = 20 })
            .AddWhere("category_id = @CategoryId", new { CategoryId = 1 });

        var products = await connection.QueryBySpecAsync(spec, SqlDialect.Sqlite);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[yellow]Product Name[/]")
            .AddColumn(new TableColumn("[yellow]Price[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]Stock[/]").RightAligned());

        foreach (var product in products)
        {
            table.AddRow(product.Name, $"[green]${product.Price:F2}[/]", $"[cyan]{product.Stock}[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Query: WHERE price > $30 AND stock > 20 AND category_id = 1[/]\n");
    }
}