# Dapper.Specifications

[![NuGet](https://img.shields.io/nuget/v/Dapper.Specifications.svg)](https://www.nuget.org/packages/Dapper.Specifications/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)

A powerful **Specification Pattern** implementation for **Dapper ORM** with multi-database support. Build dynamic, reusable, and testable database queries with a fluent API.

## Features

- **Specification Pattern** - Encapsulate query logic in reusable, testable specifications
- **Multi-Database Support** - SQL Server, PostgreSQL, MySQL, SQLite
- **Fluent API** - Chainable methods for building complex queries
- **Complex Queries** - WHERE (AND/OR), JOIN, ORDER BY, GROUP BY, HAVING
- **Pagination** - Built-in skip/take support
- **Multi-Mapping** - Support for JOIN queries with 2-7 types
- **Async/Sync** - Both asynchronous and synchronous methods
- **Type-Safe** - Full nullable reference type support
- **Input Validation** - Comprehensive validation with helpful error messages
- **CancellationToken** - Full cancellation support for async operations

## Installation

```bash
dotnet add package Dapper.Specifications
```

## Quick Start

### 1. Define Your Specification

```csharp
using Dapper.Specifications.Specifications;

public class ProductSpecification : Specification<Product>
{
    public ProductSpecification()
    {
        TableName = "products";
        PrimaryKey = "product_id";
    }
}
```

### 2. Build Your Query

```csharp
var spec = new ProductSpecification()
    .AddWhere("price > @MinPrice", new { MinPrice = 100 })
    .AddWhere("category = @Category", new { Category = "Electronics" })
    .AddOrder("price DESC")
    .SetPaging(0, 10);
```

### 3. Execute the Query

```csharp
using Dapper.Specifications.Dialects;
using Dapper.Specifications.Specifications;

// Query
var products = await connection.QueryBySpecAsync(spec, SqlDialect.SqlServer);

// Count
var count = await connection.CountBySpecAsync(spec, SqlDialect.SqlServer);

// Exists
var exists = await connection.ExistsBySpecAsync(spec, SqlDialect.SqlServer);
```

## Documentation

### Basic Usage

#### WHERE Clauses (AND)

```csharp
var spec = new ProductSpecification()
    .AddWhere("price > @MinPrice", new { MinPrice = 100 })
    .AddWhere("stock > @MinStock", new { MinStock = 0 })
    .AddWhere("category = @Category", new { Category = "Electronics" });

// Generates: WHERE price > @MinPrice AND stock > @MinStock AND category = @Category
```

#### WHERE Clauses (OR)

```csharp
var spec = new ProductSpecification()
    .AddWhere("price > @MinPrice", new { MinPrice = 1000 })
    .AddWhereOr("category = @Category", new { Category = "Premium" });

// Generates: WHERE price > @MinPrice OR category = @Category
```

#### JOIN Clauses

```csharp
var spec = new ProductSpecification()
    .AddJoin("INNER JOIN categories c ON p.category_id = c.category_id")
    .AddJoin("LEFT JOIN suppliers s ON p.supplier_id = s.supplier_id")
    .AddWhere("c.name = @CategoryName", new { CategoryName = "Electronics" });
```

#### ORDER BY

```csharp
var spec = new ProductSpecification()
    .AddOrder("price DESC")
    .AddOrder("name ASC");

// Generates: ORDER BY price DESC, name ASC
```

#### Pagination

```csharp
var spec = new ProductSpecification()
    .SetPaging(skip: 20, take: 10); // Skip 20, take 10

// SQL Server: OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY
// PostgreSQL: LIMIT 10 OFFSET 20
// MySQL: LIMIT 10 OFFSET 20
// SQLite: LIMIT 10 OFFSET 20
```

#### GROUP BY and HAVING

```csharp
var spec = new ProductSpecification()
    .SetSelectClause("category, COUNT(*) as ProductCount, AVG(price) as AvgPrice")
    .AddGroupBy("category")
    .AddHaving("COUNT(*) > @MinCount", new { MinCount = 5 })
    .AddHaving("AVG(price) > @MinAvgPrice", new { MinAvgPrice = 100 });

// Generates:
// SELECT category, COUNT(*) as ProductCount, AVG(price) as AvgPrice
// FROM products
// GROUP BY category
// HAVING COUNT(*) > @MinCount AND AVG(price) > @MinAvgPrice
```

### Advanced Usage

#### Custom SELECT Clause

```csharp
var spec = new ProductSpecification()
    .SetSelectClause("product_id, name, price, (price * 0.9) as discounted_price")
    .AddWhere("active = @Active", new { Active = true });
```

#### Multi-Mapping (JOINs with Multiple Types)

```csharp
// 2-type mapping
var results = await connection.QueryBySpecAsync<Product, Category, ProductWithCategory>(
    spec,
    (product, category) => new ProductWithCategory
    {
        Product = product,
        Category = category
    },
    splitOn: "CategoryId",
    dialect: SqlDialect.SqlServer
);

// 3-type mapping
var results = await connection.QueryBySpecAsync<Product, Category, Supplier, ProductViewModel>(
    spec,
    (product, category, supplier) => new ProductViewModel
    {
        Product = product,
        Category = category,
        Supplier = supplier
    },
    splitOn: "CategoryId,SupplierId",
    dialect: SqlDialect.PostgreSql
);
```


#### Synchronous Methods

```csharp
// Synchronous query
var products = connection.QueryBySpec(spec, SqlDialect.MySql);

// Synchronous count
var count = connection.CountBySpec(spec, SqlDialect.MySql);

// Synchronous exists
var exists = connection.ExistsBySpec(spec, SqlDialect.MySql);
```

#### CancellationToken Support

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

var products = await connection.QueryBySpecAsync(
    spec,
    SqlDialect.SqlServer,
    cancellationToken: cts.Token
);
```

### Database Dialects

The library supports multiple database dialects:

```csharp
using Dapper.Specifications.Dialects;

// SQL Server
var dialect = SqlDialect.SqlServer;

// PostgreSQL
var dialect = SqlDialect.PostgreSql;

// MySQL
var dialect = SqlDialect.MySql;

// SQLite
var dialect = SqlDialect.Sqlite;
```

Each dialect handles:
- ✅ Identifier quoting (`[name]`, `"name"`, `` `name` ``)
- ✅ Pagination syntax (OFFSET/FETCH, LIMIT/OFFSET)
- ✅ Parameter prefixes (@, :, ?)
- ✅ EXISTS query format

### Complete Example

```csharp
using Dapper;
using Dapper.Specifications.Dialects;
using Dapper.Specifications.Specifications;
using Microsoft.Data.SqlClient;

// Define specification
public class ProductSpecification : Specification<Product>
{
    public ProductSpecification()
    {
        TableName = "products";
        PrimaryKey = "product_id";
    }
}

// Build complex query
var spec = new ProductSpecification()
    .SetSelectClause("p.*, c.name as CategoryName")
    .AddJoin("INNER JOIN categories c ON p.category_id = c.category_id")
    .AddWhere("p.price BETWEEN @MinPrice AND @MaxPrice", new { MinPrice = 100, MaxPrice = 500 })
    .AddWhere("p.stock > @MinStock", new { MinStock = 0 })
    .AddWhereOr("p.featured = @Featured", new { Featured = true })
    .AddOrder("p.price DESC")
    .SetPaging(0, 20);

// Execute
using var connection = new SqlConnection(connectionString);
var products = await connection.QueryBySpecAsync(spec, SqlDialect.SqlServer);
var totalCount = await connection.CountBySpecAsync(spec, SqlDialect.SqlServer);

Console.WriteLine($"Found {totalCount} products, showing first {products.Count()}");
```

## 🧪 Testing

The library includes comprehensive test coverage:

- ✅ **100 Unit Tests** - Core functionality and validation
- ✅ **Integration Tests** - Real database testing with PostgreSQL, SQL Server, MySQL, SQLite
- ✅ **100% Pass Rate**

Run tests:

```bash
dotnet test
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built on top of [Dapper](https://github.com/DapperLib/Dapper) - the king of Micro ORMs
- Inspired by the Specification Pattern from Domain-Driven Design

## Support

- [Documentation](https://github.com/yourusername/Dapper.SpecificationExtension)
- [Issue Tracker](https://github.com/yourusername/Dapper.SpecificationExtension/issues)
- [Discussions](https://github.com/yourusername/Dapper.SpecificationExtension/discussions)