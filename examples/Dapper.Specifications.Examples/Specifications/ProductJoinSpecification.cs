using Dapper.Specifications.Examples.Models;

namespace Dapper.Specifications.Examples.Specifications;

/// <summary>
/// Specification for Product queries with table alias support for JOINs
/// </summary>
public class ProductJoinSpecification : Specification<Product>
{
    public ProductJoinSpecification()
    {
        TableName = "products p";  // Set table name with alias for JOIN queries
        PrimaryKey = "product_id";
    }
}

