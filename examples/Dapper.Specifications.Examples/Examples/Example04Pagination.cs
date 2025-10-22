using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example04Pagination : IExample
{
    public int ExampleNumber => 4;
    public string Title => "Pagination";
    public string Description => "Skip/take with ordering";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var spec = new ProductSpecification()
            .SetSelectClause(
                "product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
            .AddOrder("price DESC")
            .SetPaging(skip: 2, take: 3);

        var products = await connection.QueryBySpecAsync(spec, SqlDialect.Sqlite);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[yellow]#[/]")
            .AddColumn("[yellow]Product Name[/]")
            .AddColumn(new TableColumn("[yellow]Price[/]").RightAligned());

        var index = 3; // Starting from 3 because we skipped 2
        foreach (var product in products)
        {
            table.AddRow($"[dim]{index}[/]", product.Name, $"[green]${product.Price:F2}[/]");
            index++;
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Query: ORDER BY price DESC LIMIT 3 OFFSET 2[/]\n");
    }
}