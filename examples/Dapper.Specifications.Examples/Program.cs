using Dapper;
using Dapper.Specifications.Examples.Examples;
using Microsoft.Data.Sqlite;
using Spectre.Console;

// Parse command-line arguments
// Usage: dotnet run [--interactive] or dotnet run [--batch]
// Default is batch mode (false)
var interactiveMode = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase);

// Create an in-memory SQLite database for demonstration
var connectionString = "Data Source=:memory:;Mode=Memory;Cache=Shared";
await using var connection = new SqliteConnection(connectionString);
connection.Open();

// Create schema and insert data with progress
await AnsiConsole.Status()
    .StartAsync("Initializing database...", async ctx =>
    {
        ctx.Spinner(Spinner.Known.Dots);
        ctx.SpinnerStyle(Style.Parse("green"));

        ctx.Status("Creating schema...");
        await CreateSchemaAsync(connection);
        await Task.Delay(300);

        ctx.Status("Inserting sample data...");
        await InsertSampleDataAsync(connection);
        await Task.Delay(300);
    });

AnsiConsole.MarkupLine("[green]✓[/] Database initialized with sample data\n");

// Create example runner and execute based on mode
var runner = new ExampleRunner();

if (interactiveMode)
{
    // Interactive menu mode
    await runner.RunInteractiveAsync(connection);
}
else
{
    // Batch mode - run all examples
    await runner.RunAllAsync(connection);
    AnsiConsole.MarkupLine("\n[dim]Press any key to exit...[/]");
    Console.ReadKey();
}

static async Task CreateSchemaAsync(SqliteConnection connection)
{
    await connection.ExecuteAsync(@"
        CREATE TABLE categories (
            category_id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            description TEXT NOT NULL
        );

        CREATE TABLE products (
            product_id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            price REAL NOT NULL,
            stock INTEGER NOT NULL,
            category_id INTEGER NOT NULL,
            featured INTEGER NOT NULL,
            created_at TEXT NOT NULL,
            FOREIGN KEY (category_id) REFERENCES categories(category_id)
        );
    ");
}

static async Task InsertSampleDataAsync(SqliteConnection connection)
{
    // Insert categories
    await connection.ExecuteAsync(@"
        INSERT INTO categories (name, description) VALUES
        ('Electronics', 'Electronic devices and accessories'),
        ('Books', 'Physical and digital books'),
        ('Clothing', 'Apparel and accessories');
    ");

    // Insert products
    await connection.ExecuteAsync(@"
        INSERT INTO products (name, price, stock, category_id, featured, created_at) VALUES
        ('Laptop', 999.99, 10, 1, 1, '2024-01-01'),
        ('Mouse', 29.99, 50, 1, 0, '2024-01-02'),
        ('Keyboard', 79.99, 30, 1, 1, '2024-01-03'),
        ('C# Programming', 49.99, 20, 2, 1, '2024-01-04'),
        ('SQL Mastery', 39.99, 15, 2, 0, '2024-01-05'),
        ('T-Shirt', 19.99, 100, 3, 0, '2024-01-06'),
        ('Jeans', 59.99, 40, 3, 1, '2024-01-07');
    ");
}
