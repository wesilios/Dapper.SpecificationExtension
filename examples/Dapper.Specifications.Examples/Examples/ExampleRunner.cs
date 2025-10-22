using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace Dapper.Specifications.Examples.Examples;

/// <summary>
/// Manages running examples with support for both batch and interactive modes
/// </summary>
public class ExampleRunner
{
    private readonly List<IExample> _examples;

    public ExampleRunner()
    {
        _examples = new List<IExample>
        {
            new Example01BasicQuery(),
            new Example02MultipleWhereAnd(),
            new Example03WhereOr(),
            new Example04Pagination(),
            new Example05Ordering(),
            new Example06CustomSelect(),
            new Example07Count(),
            new Example08Exists(),
            new Example09FluentChaining(),
            new Example10SynchronousMethods(),
            new Example11GroupByHaving(),
            new Example12Distinct(),
            new Example13Join(),
            new Example14Union(),
            new Example15Subquery()
        };
    }

    /// <summary>
    /// Runs all examples in batch mode
    /// </summary>
    public async Task RunAllAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(
            new FigletText("Dapper.Specifications")
                .LeftJustified()
                .Color(Color.Blue));

        AnsiConsole.Write(
            new Rule("[yellow]Running All Examples[/]")
                .RuleStyle("grey")
                .LeftJustified());

        AnsiConsole.MarkupLine("");

        foreach (var example in _examples)
        {
            await example.ExecuteAsync(connection);
        }

        AnsiConsole.Write(
            new Rule("[green]All Examples Completed Successfully![/]")
                .RuleStyle("green")
                .LeftJustified());
    }

    /// <summary>
    /// Runs examples in interactive menu mode
    /// </summary>
    public async Task RunInteractiveAsync(SqliteConnection connection)
    {
        AnsiConsole.Write(
            new FigletText("Dapper.Specifications")
                .LeftJustified()
                .Color(Color.Blue));

        AnsiConsole.Write(
            new Rule("[yellow]Interactive Example Menu[/]")
                .RuleStyle("grey")
                .LeftJustified());

        AnsiConsole.MarkupLine("");

        while (true)
        {
            DisplayMenu();

            try
            {
                var choices = _examples.Select(e => $"{e.ExampleNumber:D2}. {e.Title}").ToList();
                choices.Add("Exit");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Select an example to run:[/]")
                        .AddChoices(choices));

                if (choice == "Exit")
                {
                    AnsiConsole.MarkupLine("[green]Thank you for using Dapper.Specifications Examples![/]");
                    break;
                }

                var exampleNumber = int.Parse(choice.Substring(0, 2));
                var example = _examples.FirstOrDefault(e => e.ExampleNumber == exampleNumber);

                if (example != null)
                {
                    AnsiConsole.MarkupLine("");
                    await example.ExecuteAsync(connection);
                    AnsiConsole.MarkupLine("[dim]Press Enter to continue...[/]");
                    Console.ReadLine();
                    AnsiConsole.Clear();
                }
            }
            catch (NotSupportedException)
            {
                // Terminal doesn't support interactive prompts (e.g., piped input)
                AnsiConsole.MarkupLine("[yellow]Interactive mode not supported in this terminal.[/]");
                AnsiConsole.MarkupLine("[yellow]Enter example number (01-15) or 'exit' to quit:[/]");

                var input = Console.ReadLine()?.Trim() ?? "";

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    AnsiConsole.MarkupLine("[green]Thank you for using Dapper.Specifications Examples![/]");
                    break;
                }

                if (int.TryParse(input, out var exampleNumber))
                {
                    var example = _examples.FirstOrDefault(e => e.ExampleNumber == exampleNumber);
                    if (example != null)
                    {
                        AnsiConsole.MarkupLine("");
                        await example.ExecuteAsync(connection);
                        AnsiConsole.MarkupLine("[dim]Press Enter to continue...[/]");
                        Console.ReadLine();
                        AnsiConsole.Clear();
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Invalid example number.[/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid input. Please enter a number between 01-15 or 'exit'.[/]");
                }
            }
        }
    }

    private void DisplayMenu()
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title("[bold yellow]Available Examples[/]")
            .AddColumn("[yellow]#[/]")
            .AddColumn("[yellow]Title[/]")
            .AddColumn("[yellow]Description[/]");

        foreach (var example in _examples)
        {
            table.AddRow(
                $"[cyan]{example.ExampleNumber:D2}[/]",
                $"[green]{example.Title}[/]",
                $"[dim]{example.Description}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("");
    }
}