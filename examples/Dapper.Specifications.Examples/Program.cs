using Dapper;
using Dapper.Specifications.Dialects;
using Dapper.Specifications.Evaluators;
using Dapper.Specifications.Examples.Specifications;
using Dapper.Specifications.Specifications;
using Microsoft.Data.Sqlite;
using Spectre.Console;

// Display header
AnsiConsole.Write(
    new FigletText("Dapper.Specifications")
        .LeftJustified()
        .Color(Color.Blue));

AnsiConsole.Write(
    new Rule("[yellow]Examples & Demonstrations[/]")
        .RuleStyle("grey")
        .LeftJustified());

AnsiConsole.MarkupLine("");

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

// Example 1: Basic Query with WHERE clause
await Example1_BasicQuery(connection);

// Example 2: Multiple WHERE conditions (AND)
await Example2_MultipleWhereAnd(connection);

// Example 3: OR conditions
await Example3_WhereOr(connection);

// Example 4: Pagination
await Example4_Pagination(connection);

// Example 5: Ordering
await Example5_Ordering(connection);

// Example 6: Custom SELECT clause
await Example6_CustomSelect(connection);

// Example 7: COUNT query
await Example7_Count(connection);

// Example 8: EXISTS query
await Example8_Exists(connection);

// Example 9: Fluent API chaining
await Example9_FluentChaining(connection);

// Example 10: Synchronous methods
Example10_SynchronousMethods(connection);

// Final summary
AnsiConsole.Write(
    new Rule("[green]All Examples Completed Successfully![/]")
        .RuleStyle("green")
        .LeftJustified());

AnsiConsole.MarkupLine("\n[dim]Press any key to exit...[/]");
Console.ReadKey();

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

static async Task Example1_BasicQuery(SqliteConnection connection)
{
    AnsiConsole.Write(new Rule("[blue]Example 1: Basic Query with WHERE clause[/]").LeftJustified());

    var spec = new ProductSpecification()
        .SetSelectClause("product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
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

static async Task Example2_MultipleWhereAnd(SqliteConnection connection)
{
    AnsiConsole.Write(new Rule("[blue]Example 2: Multiple WHERE conditions (AND)[/]").LeftJustified());

    var spec = new ProductSpecification()
        .SetSelectClause("product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
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

static async Task Example3_WhereOr(SqliteConnection connection)
{
    AnsiConsole.Write(new Rule("[blue]Example 3: OR conditions[/]").LeftJustified());

    var spec = new ProductSpecification()
        .SetSelectClause("product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
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


static async Task Example4_Pagination(SqliteConnection connection)
{
    AnsiConsole.Write(new Rule("[blue]Example 4: Pagination[/]").LeftJustified());

    var spec = new ProductSpecification()
        .SetSelectClause("product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
        .AddOrder("price DESC")
        .SetPaging(skip: 2, take: 3);

    var products = await connection.QueryBySpecAsync(spec, SqlDialect.Sqlite);

    var table = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Grey)
        .AddColumn("[yellow]#[/]")
        .AddColumn("[yellow]Product Name[/]")
        .AddColumn(new TableColumn("[yellow]Price[/]").RightAligned());

    int index = 3; // Starting from 3 because we skipped 2
    foreach (var product in products)
    {
        table.AddRow($"[dim]{index}[/]", product.Name, $"[green]${product.Price:F2}[/]");
        index++;
    }

    AnsiConsole.Write(table);
    AnsiConsole.MarkupLine("[dim]Query: ORDER BY price DESC LIMIT 3 OFFSET 2[/]\n");
}

static async Task Example5_Ordering(SqliteConnection connection)
{
    AnsiConsole.Write(new Rule("[blue]Example 5: Multi-column Ordering[/]").LeftJustified());

    var spec = new ProductSpecification()
        .SetSelectClause("product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
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

static async Task Example6_CustomSelect(SqliteConnection connection)
{
    AnsiConsole.Write(new Rule("[blue]Example 6: Custom SELECT with Calculated Fields[/]").LeftJustified());

    var spec = new ProductSpecification()
        .SetSelectClause("name AS Name, price AS Price, (price * 0.9) as DiscountedPrice")
        .AddWhere("price > @MinPrice", new { MinPrice = 50 });

    var results = await connection.QueryAsync(
        SpecificationEvaluator.Build(spec, SqlDialect.Sqlite).Sql,
        spec.Parameters
    );

    var table = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Grey)
        .AddColumn("[yellow]Product Name[/]")
        .AddColumn(new TableColumn("[yellow]Original Price[/]").RightAligned())
        .AddColumn(new TableColumn("[yellow]Discounted (10%)[/]").RightAligned())
        .AddColumn(new TableColumn("[yellow]Savings[/]").RightAligned());

    foreach (var result in results)
    {
        string name = result.Name;
        double price = result.Price;
        double discountedPrice = result.DiscountedPrice;
        var savings = price - discountedPrice;
        table.AddRow(
            name,
            $"[dim]${price:F2}[/]",
            $"[green]${discountedPrice:F2}[/]",
            $"[red]-${savings:F2}[/]"
        );
    }

    AnsiConsole.Write(table);
    AnsiConsole.MarkupLine("[dim]Query: SELECT name, price, (price * 0.9) as DiscountedPrice WHERE price > $50[/]\n");
}

static async Task Example7_Count(SqliteConnection connection)
{
    AnsiConsole.Write(new Rule("[blue]Example 7: COUNT Query[/]").LeftJustified());

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

static async Task Example8_Exists(SqliteConnection connection)
{
    AnsiConsole.Write(new Rule("[blue]Example 8: EXISTS Query[/]").LeftJustified());

    var priceChecks = new[] { 1000, 500, 100, 50, 10 };

    var table = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Grey)
        .AddColumn("[yellow]Price Threshold[/]")
        .AddColumn(new TableColumn("[yellow]Products Exist?[/]").Centered());

    foreach (var price in priceChecks)
    {
        var spec = new ProductSpecification()
            .AddWhere("price > @Price", new { Price = price });

        var exists = await connection.ExistsBySpecAsync(spec, SqlDialect.Sqlite);
        var statusIcon = exists ? "[green]✓ Yes[/]" : "[red]✗ No[/]";
        table.AddRow($"[cyan]> ${price}[/]", statusIcon);
    }

    AnsiConsole.Write(table);
    AnsiConsole.MarkupLine("[dim]Query: SELECT EXISTS(SELECT 1 FROM products WHERE price > @Price)[/]\n");
}

static async Task Example9_FluentChaining(SqliteConnection connection)
{
    AnsiConsole.Write(new Rule("[blue]Example 9: Fluent API Chaining[/]").LeftJustified());

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
        .SetSelectClause("product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
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

static void Example10_SynchronousMethods(SqliteConnection connection)
{
    AnsiConsole.Write(new Rule("[blue]Example 10: Synchronous Methods[/]").LeftJustified());

    var spec = new ProductSpecification()
        .SetSelectClause("product_id AS ProductId, name AS Name, price AS Price, stock AS Stock, category_id AS CategoryId, featured AS Featured, created_at AS CreatedAt")
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
}
