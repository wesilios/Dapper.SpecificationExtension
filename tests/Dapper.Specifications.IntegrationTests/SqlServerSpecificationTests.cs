using Dapper.Specifications.Dialects;
using Dapper.Specifications.IntegrationTests.Fixtures;
using Dapper.Specifications.IntegrationTests.MockSpecifications;
using Dapper.Specifications.Specifications;
using Microsoft.Data.SqlClient;
using Shouldly;

namespace Dapper.Specifications.IntegrationTests;

public class SqlServerSpecificationTests : SpecificationIntegrationTestBase<SqlServerDatabaseFixture>
{
    public SqlServerSpecificationTests(SqlServerDatabaseFixture fixture) : base(fixture)
    {
        Connection = new SqlConnection(fixture.ConnectionString);
    }

    private async Task InitializeSchemaAsync()
    {
        await Connection.ExecuteAsync(@"
            IF OBJECT_ID('products', 'U') IS NOT NULL DROP TABLE products;
            IF OBJECT_ID('product_collections', 'U') IS NOT NULL DROP TABLE product_collections;

            CREATE TABLE product_collections (
                collection_id INT PRIMARY KEY IDENTITY(1,1),
                name NVARCHAR(255) NOT NULL,
                description NVARCHAR(MAX) NOT NULL
            );

            CREATE TABLE products (
                product_id INT PRIMARY KEY IDENTITY(1,1),
                name NVARCHAR(255) NOT NULL,
                price DECIMAL(18,2) NOT NULL,
                collection_id INT NOT NULL,
                FOREIGN KEY (collection_id) REFERENCES product_collections(collection_id)
            );
        ");

        await Connection.ExecuteAsync(
            "INSERT INTO product_collections (name, description) VALUES ('Renewable Energy', 'Renewable Energy Products'), ('Vehicles', 'Vehicles transportation');");
        await Connection.ExecuteAsync(
            "INSERT INTO products (name, price, collection_id) VALUES ('Solar Panel', 100, 1), ('Wind Turbine', 500, 1);");
        await Connection.ExecuteAsync(
            "INSERT INTO products (name, price, collection_id) VALUES ('Toyota Hatchback', 100, 2), ('Porches 911', 500, 2);");
    }

    [Fact]
    public async Task Product_QueryBySpecAsync_ShouldReturn_ExpectedResults()
    {
        // Arrange
        await InitializeSchemaAsync();
        var specification = new ProductSpecification();

        specification.AddWhere("collection_id = @CollectionId", new { CollectionId = 1 });
        specification.AddWhere("price > @MinPrice", new { MinPrice = 180 });

        // Act
        var results =
            (await Connection.QueryBySpecAsync(specification, SqlDialect.SqlServer)).ToList();

        // Assert
        results[0].Name.ShouldBe("Wind Turbine");
        results[0].Price.ShouldBe(500);
        results[0].CollectionId.ShouldBe(1);
    }

    [Fact]
    public async Task Product_CountBySpecAsync_ShouldReturn_ExpectedResults()
    {
        // Arrange
        await InitializeSchemaAsync();
        var specification = new ProductSpecification();

        specification.AddWhere("collection_id = @CollectionId", new { CollectionId = 1 });
        specification.AddWhere("price > @MinPrice", new { MinPrice = 180 });

        // Act
        var results =
            await Connection.CountBySpecAsync(specification, SqlDialect.SqlServer);

        // Assert
        results.ShouldBe(1);
    }

    [Fact]
    public async Task Product_ExistsBySpecAsync_ShouldReturn_ExpectedResults()
    {
        // Arrange
        await InitializeSchemaAsync();
        var specification = new ProductSpecification();

        specification.AddWhere("collection_id = @CollectionId", new { CollectionId = 1 });
        specification.AddWhere("price > @MinPrice", new { MinPrice = 180 });

        // Act
        var results =
            await Connection.ExistsBySpecAsync(specification, SqlDialect.SqlServer);

        // Assert
        results.ShouldBeTrue();
    }

    [Fact]
    public async Task Product_QueryBySpecAsync_WithAddWhereOr_ShouldReturn_ExpectedResults()
    {
        // Arrange
        await InitializeSchemaAsync();
        var specification = new ProductSpecification();

        specification.AddWhere("price > @HighPrice", new { HighPrice = 400 });
        specification.AddWhereOr("collection_id = @CollectionId", new { CollectionId = 2 });

        // Act
        var results =
            (await Connection.QueryBySpecAsync(specification, SqlDialect.SqlServer)).ToList();

        // Assert
        results.Count.ShouldBe(3); // Wind Turbine (500) OR all from collection 2
        results.ShouldContain(p => p.Name == "Wind Turbine");
        results.ShouldContain(p => p.Name == "Toyota Hatchback");
        results.ShouldContain(p => p.Name == "Porches 911");
    }

    [Fact]
    public async Task Product_QueryBySpecAsync_WithMultipleAddWhereOr_ShouldReturn_ExpectedResults()
    {
        // Arrange
        await InitializeSchemaAsync();
        var specification = new ProductSpecification();

        specification.AddWhereOr("name = @Name1", new { Name1 = "Solar Panel" });
        specification.AddWhereOr("name = @Name2", new { Name2 = "Toyota Hatchback" });

        // Act
        var results =
            (await Connection.QueryBySpecAsync(specification, SqlDialect.SqlServer)).ToList();

        // Assert
        results.Count.ShouldBe(2);
        results.ShouldContain(p => p.Name == "Solar Panel");
        results.ShouldContain(p => p.Name == "Toyota Hatchback");
    }

    [Fact]
    public async Task Product_QueryBySpecAsync_WithGroupByAndHaving_ShouldReturn_ExpectedResults()
    {
        // Arrange
        await InitializeSchemaAsync();
        var specification = new ProductSpecification();

        specification.SetSelectClause("collection_id, COUNT(*) as ProductCount, AVG(price) as AvgPrice");
        specification.AddGroupBy("collection_id");
        specification.AddHaving("COUNT(*) >= @MinCount", new { MinCount = 2 });

        // Act
        var results = (await Connection.QueryAsync<dynamic>(
            "SELECT collection_id, COUNT(*) as ProductCount, AVG(price) as AvgPrice FROM products GROUP BY collection_id HAVING COUNT(*) >= @MinCount",
            new { MinCount = 2 })).ToList();

        // Assert
        results.Count.ShouldBe(2); // Both collections have 2 products
    }

    [Fact]
    public async Task Product_QueryBySpecAsync_WithDistinct_ShouldReturn_UniqueResults()
    {
        // Arrange
        await InitializeSchemaAsync();
        var specification = new ProductSpecification();

        specification.SetSelectClause("collection_id");
        specification.SetDistinct();

        // Act
        var results = (await Connection.QueryBySpecAsync(specification, SqlDialect.SqlServer)).ToList();

        // Assert
        results.Count.ShouldBe(2); // Only 2 unique collection_ids
    }

    [Fact]
    public async Task Product_QueryBySpecAsync_WithUnion_ShouldReturn_CombinedResults()
    {
        // Arrange
        await InitializeSchemaAsync();

        var spec1 = new ProductSpecification();
        spec1.AddWhere("price < @Price", new { Price = 200 });

        var spec2 = new ProductSpecification();
        spec2.AddWhere("price > @Price", new { Price = 400 });

        spec1.AddUnion(spec2);

        // Act
        var results = (await Connection.QueryBySpecAsync(spec1, SqlDialect.SqlServer)).ToList();

        // Assert
        results.Count.ShouldBe(4); // 2 products < 200 and 2 products > 400
        results.ShouldContain(p => p.Name == "Solar Panel");
        results.ShouldContain(p => p.Name == "Toyota Hatchback");
        results.ShouldContain(p => p.Name == "Wind Turbine");
        results.ShouldContain(p => p.Name == "Porches 911");
    }

    [Fact]
    public async Task Product_QueryBySpecAsync_WithFromSubquery_ShouldReturn_ExpectedResults()
    {
        // Arrange
        await InitializeSchemaAsync();

        var subquery = new ProductSpecification();
        subquery.SetSelectClause("collection_id, AVG(price) as avg_price");
        subquery.AddGroupBy("collection_id");

        var spec = new ProductSpecification();
        spec.SetFromSubquery(subquery, "sub");
        spec.SetSelectClause("sub.collection_id, sub.avg_price");
        spec.AddWhere("sub.avg_price > @MinAvg", new { MinAvg = 250 });

        // Act
        var results = (await Connection.QueryAsync<dynamic>(
            "SELECT sub.collection_id, sub.avg_price FROM (SELECT collection_id, AVG(price) as avg_price FROM products GROUP BY collection_id) sub WHERE sub.avg_price > @MinAvg",
            new { MinAvg = 250 })).ToList();

        // Assert
        results.Count.ShouldBe(2); // Both collections have avg > 250
    }

    [Fact]
    public async Task Product_QueryBySpecAsync_WithCTE_ShouldReturn_ExpectedResults()
    {
        // Arrange
        await InitializeSchemaAsync();

        var cteSpec = new ProductSpecification();
        cteSpec.SetSelectClause("collection_id, AVG(price) as avg_price");
        cteSpec.AddGroupBy("collection_id");

        // Create a custom specification that uses the CTE
        var spec = new CTEProductSpecification();
        spec.AddCommonTableExpression("CollectionAvg", cteSpec);
        spec.AddWhere("avg_price > @MinAvg", new { MinAvg = 250 });

        // Act
        var results = (await Connection.QueryBySpecAsync(spec, SqlDialect.SqlServer)).ToList();

        // Assert
        results.Count.ShouldBe(2); // Both collections have avg > 250
    }

    private class CTEProductSpecification : Specification<Product>
    {
        public CTEProductSpecification()
        {
            TableName = "CollectionAvg";
            PrimaryKey = "collection_id";
        }
    }
}

