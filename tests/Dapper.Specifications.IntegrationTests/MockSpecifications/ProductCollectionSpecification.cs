using Dapper.Specifications.Specifications;

namespace Dapper.Specifications.IntegrationTests.MockSpecifications;

public class ProductCollectionSpecification : Specification<ProductCollection>
{
    public ProductCollectionSpecification()
    {
        TableName = "product_collections pc";
        PrimaryKey = "collection_id";
    }
}