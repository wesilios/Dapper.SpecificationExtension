# Dapper.Specifications Examples

This project demonstrates all the features of the **Dapper.Specifications** library with beautiful, interactive examples using [Spectre.Console](https://spectreconsole.net/).

## What's Included

This examples project showcases:

### 15 Comprehensive Examples

1. **Basic Query with WHERE clause** - Simple filtering with parameters
2. **Multiple WHERE conditions (AND)** - Combining multiple filters
3. **OR conditions** - Using `AddWhereOr()` for alternative conditions
4. **Pagination** - Skip/take with ordering
5. **Multi-column Ordering** - Sorting by multiple columns
6. **Custom SELECT with Calculated Fields** - Dynamic field calculations
7. **COUNT Query** - Counting records by category
8. **EXISTS Query** - Checking for record existence
9. **Fluent API Chaining** - Complex queries with method chaining
10. **Synchronous Methods** - Non-async alternatives
11. **GROUP BY and HAVING** - Aggregate functions with filtering
12. **DISTINCT** - Selecting unique values
13. **INNER JOIN** - Joining multiple tables
14. **UNION** - Combining results from multiple queries
15. **Subquery in FROM** - Using subqueries as data sources

### Beautiful Console UI

- **ASCII Art Header** - Eye-catching title using Figlet
- **Animated Spinners** - Progress indicators during database operations
- **Styled Tables** - Color-coded data with borders
- **Icons & Symbols** - Visual indicators (star for featured, checkmark/cross for booleans)
- **Code Panels** - Syntax-highlighted code examples
- **Color Coding** - Different colors for different data types

## Running the Examples

### Prerequisites

- .NET 8.0 SDK or later

### Run Modes

The examples application supports two modes:

#### Batch Mode (Default)

Runs all 15 examples sequentially from start to finish. Perfect for a complete demonstration or automated testing.

```bash
# From the repository root
dotnet run --project examples/Dapper.Specifications.Examples/Dapper.Specifications.Examples.csproj

# Or from the examples directory
cd examples/Dapper.Specifications.Examples
dotnet run

# Explicitly specify batch mode
dotnet run --batch
```

#### Interactive Mode

Displays an interactive menu where you can select and run individual examples. Great for exploring specific features or learning at your own pace.

```bash
# From the repository root
dotnet run --project examples/Dapper.Specifications.Examples/Dapper.Specifications.Examples.csproj -- --interactive

# Or from the examples directory
cd examples/Dapper.Specifications.Examples
dotnet run -- --interactive
```

**Interactive Mode Features:**
- **Selection Menu** - Browse all 15 examples with descriptions in a formatted table
- **Run Individual Examples** - Choose any example to run independently
- **Repeat Examples** - Run the same example multiple times to study it
- **Easy Navigation** - Return to the menu after each example completes
- **Graceful Exit** - Select "Exit" from the menu when done

**Fallback for Non-Interactive Terminals:**
If your terminal doesn't support interactive prompts (e.g., piped input), the application automatically falls back to a text-based input mode where you can type example numbers (01-15) or 'exit' to quit.

## Dependencies

- **Dapper.Specifications** - The main library (project reference)
- **Microsoft.Data.Sqlite** - In-memory SQLite database for demonstrations
- **Spectre.Console** - Beautiful console UI framework

## Example Highlights

### Example 1: Basic Query

```csharp
var spec = new ProductSpecification()
    .SetSelectClause("product_id AS ProductId, name AS Name, price AS Price, ...")
    .AddWhere("price > @MinPrice", new { MinPrice = 50 });

var products = await connection.QueryBySpecAsync(spec, SqlDialect.Sqlite);
```

### Example 3: OR Conditions

```csharp
var spec = new ProductSpecification()
    .SetSelectClause("...")
    .AddWhere("price > @HighPrice", new { HighPrice = 500 })
    .AddWhereOr("featured = @Featured", new { Featured = 1 });
```

### Example 9: Fluent API Chaining

```csharp
var spec = new ProductSpecification()
    .SetSelectClause("...")
    .AddWhere("price BETWEEN @MinPrice AND @MaxPrice", new { MinPrice = 30, MaxPrice = 100 })
    .AddWhere("stock > @MinStock", new { MinStock = 15 })
    .AddWhereOr("featured = @Featured", new { Featured = 1 })
    .AddOrder("price DESC")
    .SetPaging(0, 5);
```

### Example 11: GROUP BY and HAVING

```csharp
var spec = new ProductSpecification()
    .SetSelectClause("category_id, COUNT(*) as ProductCount, AVG(price) as AvgPrice, MIN(price) as MinPrice, MAX(price) as MaxPrice")
    .AddGroupBy("category_id")
    .AddHaving("COUNT(*) > @MinCount", new { MinCount = 1 })
    .AddOrder("AvgPrice DESC");

var results = await connection.QueryAsync(
    SpecificationEvaluator.Build(spec, SqlDialect.Sqlite).Sql,
    spec.Parameters
);
```

### Example 12: DISTINCT

```csharp
var spec = new ProductSpecification()
    .SetDistinct()
    .SetSelectClause("category_id")
    .AddOrder("category_id ASC");

var results = await connection.QueryAsync(
    SpecificationEvaluator.Build(spec, SqlDialect.Sqlite).Sql,
    spec.Parameters
);
```

### Example 13: INNER JOIN

```csharp
var spec = new ProductSpecification()
    .SetSelectClause("p.product_id, p.name, p.price, c.name as CategoryName")
    .AddInnerJoin("categories c ON p.category_id = c.category_id")
    .AddWhere("p.price > @MinPrice", new { MinPrice = 50 })
    .AddOrder("p.price DESC");

var results = await connection.QueryAsync(
    SpecificationEvaluator.Build(spec, SqlDialect.Sqlite).Sql,
    spec.Parameters
);
```

### Example 14: UNION

```csharp
// Expensive products
var spec1 = new ProductSpecification()
    .SetSelectClause("name, price, 'Expensive' AS Category")
    .AddWhere("price > @HighPrice", new { HighPrice = 500 });

// Featured products
var spec2 = new ProductSpecification()
    .SetSelectClause("name, price, 'Featured' AS Category")
    .AddWhere("featured = @Featured", new { Featured = 1 });

spec1.AddUnion(spec2);

var results = await connection.QueryAsync(
    SpecificationEvaluator.Build(spec1, SqlDialect.Sqlite).Sql,
    spec1.Parameters
);
```

### Example 15: Subquery in FROM

```csharp
// Subquery: Get average price per category
var subquery = new ProductSpecification()
    .SetSelectClause("category_id, AVG(price) as avg_price")
    .AddGroupBy("category_id");

// Main query: Use subquery
var spec = new ProductSpecification()
    .SetSelectClause("CategoryId, AvgPrice")
    .SetFromSubquery(subquery, "cat_avg")
    .AddWhere("AvgPrice > @MinAvg", new { MinAvg = 50 })
    .AddOrder("AvgPrice DESC");

var results = await connection.QueryAsync(
    SpecificationEvaluator.Build(spec, SqlDialect.Sqlite).Sql,
    spec.Parameters
);
```

## Sample Data

The examples use an in-memory SQLite database with:

- **3 Categories**: Electronics, Books, Clothing
- **7 Products**: Laptop, Mouse, Keyboard, C# Programming, SQL Mastery, T-Shirt, Jeans

All data is created automatically when you run the examples.

## Learning Path

We recommend running the examples in order:

1. **Beginner Level** - Start with **Examples 1-5** to understand basic queries, filtering, and sorting
   - Example 1: Basic WHERE clause
   - Example 2: Multiple AND conditions
   - Example 3: OR conditions
   - Example 4: Pagination
   - Example 5: Multi-column ordering

2. **Intermediate Level** - Progress through **Examples 6-10** for advanced queries and synchronous methods
   - Example 6: Custom SELECT with calculations
   - Example 7: COUNT queries
   - Example 8: EXISTS queries
   - Example 9: Fluent API chaining
   - Example 10: Synchronous alternatives

3. **Advanced Level** - Explore **Examples 11-15** for complex query patterns
   - Example 11: GROUP BY with aggregate functions and HAVING
   - Example 12: DISTINCT for unique results
   - Example 13: INNER JOIN with multiple tables
   - Example 14: UNION to combine query results
   - Example 15: Subqueries in FROM clause

## Tips

- The examples use **SQLite** dialect, but the same code works with SQL Server, PostgreSQL, and MySQL
- All queries are **parameterized** to prevent SQL injection
- The **fluent API** makes complex queries readable and maintainable
- Both **async** and **sync** methods are available

## Related Resources

- [Main README](../../README.md) - Full library documentation
- [Spectre.Console Documentation](https://spectreconsole.net/) - Learn about the UI framework
- [Dapper Documentation](https://github.com/DapperLib/Dapper) - Learn about Dapper ORM

## License

This examples project is part of Dapper.Specifications and is licensed under the MIT License.

