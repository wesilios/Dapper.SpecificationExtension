using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example09FluentChaining : IExample
{
    public int ExampleNumber => 9;
    public string Title => "Fluent API Chaining";
    public string Description => "Complex queries with method chaining";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        // Display the fluent chain as a panel
        var codePanel = new Panel(
            "[grey]var spec = new ProductSpecification()[/]\n" +
            "    [cyan].SetSelectClause[/]([green]\"...\"[/])\n" +
            "    [cyan].AddWhere[/]([green]\"price BETWEEN @MinPrice AND @MaxPrice\"[/], ...)\n" +
            "    [cyan].AddWhere[/]([green]\"stock > @MinStock\"[/], ...)\n" +
            "    [cyan].AddWhereOr[/]([green]\"featured = @Featured\"[/], ...)\n" +
            "    [cyan].AddOrder[/]([green]\"price DESC\"[/])\n" +
            "    [cyan].SetPaging[/]([yellow]0[/], [yellow]5[/]);"
        )
        {
            Header = new PanelHeader("[yellow]Fluent Method Chain[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey)
        };

        AnsiConsole.Write(codePanel);
        AnsiConsole.WriteLine();

        // Build a complex query using fluent chaining
        var spec = new ProductSpecification()
            .SetSelectClause(
                "product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
            .AddWhere("price BETWEEN @MinPrice AND @MaxPrice", new { MinPrice = 30, MaxPrice = 100 })
            .AddWhere("stock > @MinStock", new { MinStock = 15 })
            .AddWhereOr("featured = @Featured", new { Featured = 1 })
            .AddOrder("price DESC")
            .SetPaging(0, 5);

        var products = await connection.QueryBySpecAsync(spec, SqlDialect.Sqlite);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[yellow]Product Name[/]")
            .AddColumn(new TableColumn("[yellow]Price[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]Stock[/]").RightAligned())
            .AddColumn(new TableColumn("[yellow]Featured[/]").Centered());

        foreach (var product in products)
        {
            var featuredIcon = product.Featured ? "[green]★[/]" : "[dim]☆[/]";
            table.AddRow(
                product.Name,
                $"[green]${product.Price:F2}[/]",
                $"[cyan]{product.Stock}[/]",
                featuredIcon
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Complex query combining WHERE, OR, ORDER BY, and LIMIT[/]\n");
    }
}