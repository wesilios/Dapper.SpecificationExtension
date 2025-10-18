# Dapper.Specifications Examples

This project demonstrates all the features of the **Dapper.Specifications** library with beautiful, interactive examples using [Spectre.Console](https://spectreconsole.net/).

## What's Included

This examples project showcases:

### 10 Comprehensive Examples

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

### Run

```bash
# From the repository root
dotnet run --project examples/Dapper.Specifications.Examples/Dapper.Specifications.Examples.csproj

# Or from the examples directory
cd examples/Dapper.Specifications.Examples
dotnet run
```

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

## Sample Data

The examples use an in-memory SQLite database with:

- **3 Categories**: Electronics, Books, Clothing
- **7 Products**: Laptop, Mouse, Keyboard, C# Programming, SQL Mastery, T-Shirt, Jeans

All data is created automatically when you run the examples.

## Learning Path

We recommend running the examples in order:

1. Start with **Example 1** to understand basic queries
2. Progress through **Examples 2-5** to learn filtering and sorting
3. Explore **Examples 6-8** for advanced queries (COUNT, EXISTS, custom SELECT)
4. Study **Example 9** to see how to combine multiple features
5. Check **Example 10** for synchronous alternatives

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

