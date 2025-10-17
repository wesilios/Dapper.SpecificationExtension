using Dapper.Specifications.Specifications;

namespace Dapper.Specifications.IntegrationTests.MockSpecifications;

public class ProductSpecification : Specification<Product>
{
    public ProductSpecification()
    {
        TableName = "products";
        PrimaryKey = "product_id";
    }
}