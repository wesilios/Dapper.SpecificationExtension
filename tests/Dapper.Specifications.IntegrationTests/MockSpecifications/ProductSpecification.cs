namespace Dapper.Specifications.IntegrationTests.MockSpecifications;

public class ProductSpecification : Specification<Product>
{
    public ProductSpecification(bool useSqliteAliases = false)
    {
        TableName = "products";
        PrimaryKey = "product_id";

        // SQLite needs column aliases for proper mapping
        if (useSqliteAliases)
        {
            SetSelectClause("product_id AS ProductId, name AS Name, price AS Price, collection_id AS CollectionId");
        }
    }
}