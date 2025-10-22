using Dapper.Specifications.Examples.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

public class Example07Count : IExample
{
    public int ExampleNumber => 7;
    public string Title => "COUNT Query";
    public string Description => "Counting records by category";

    public async Task ExecuteAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(new Rule($"[blue]Example {ExampleNumber}: {Title}[/]").LeftJustified());

        var categories = new[]
        {
            (Id: 1, Name: "Electronics", Color: "cyan"),
            (Id: 2, Name: "Books", Color: "yellow"),
            (Id: 3, Name: "Clothing", Color: "magenta")
        };

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[yellow]Category[/]")
            .AddColumn(new TableColumn("[yellow]Product Count[/]").RightAligned());

        foreach (var category in categories)
        {
            var spec = new ProductSpecification()
                .AddWhere("category_id = @CategoryId", new { CategoryId = category.Id });

            var count = await connection.CountBySpecAsync(spec, SqlDialect.Sqlite);
            table.AddRow($"[{category.Color}]{category.Name}[/]", $"[green]{count}[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Query: SELECT COUNT(*) FROM products WHERE category_id = @CategoryId[/]\n");
    }
}