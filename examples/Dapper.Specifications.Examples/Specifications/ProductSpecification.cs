using Dapper.Specifications.Examples.Models;

namespace Dapper.Specifications.Examples.Specifications;

public class ProductSpecification : Specification<Product>
{
    public ProductSpecification()
    {
        TableName = "products";
        PrimaryKey = "product_id";
    }
}

