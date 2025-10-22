using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example05Ordering : IExample
{
    public int ExampleNumber => 5;
    public string Title => "Multi-column Ordering";
    public string Description => "Sorting by multiple columns";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var spec = new ProductSpecification()
            .SetSelectClause(
                "product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
            .AddOrder("category_id ASC")
            .AddOrder("price DESC");

        var products = await connection.QueryBySpecAsync(spec, SqlDialect.Sqlite);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[yellow]Category[/]")
            .AddColumn("[yellow]Product Name[/]")
            .AddColumn(new TableColumn("[yellow]Price[/]").RightAligned());

        var categoryNames = new Dictionary<int, string>
        {
            { 1, "Electronics" },
            { 2, "Books" },
            { 3, "Clothing" }
        };

        foreach (var product in products)
        {
            var categoryColor = product.CategoryId switch
            {
                1 => "cyan",
                2 => "yellow",
                3 => "magenta",
                _ => "white"
            };
            table.AddRow(
                $"[{categoryColor}]{categoryNames[product.CategoryId]}[/]",
                product.Name,
                $"[green]${product.Price:F2}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Query: ORDER BY category_id ASC, price DESC[/]\n");
    }
}