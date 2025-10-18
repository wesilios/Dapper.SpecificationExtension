using Dapper.Specifications.Dialects;
using Dapper.Specifications.IntegrationTests.Fixtures;
using Dapper.Specifications.IntegrationTests.MockSpecifications;
using Dapper.Specifications.Specifications;
using MySqlConnector;
using Shouldly;

namespace Dapper.Specifications.IntegrationTests;

public class MySqlSpecificationTests : SpecificationIntegrationTestBase<MySqlDatabaseFixture>
{
    public MySqlSpecificationTests(MySqlDatabaseFixture fixture) : base(fixture)
    {
        Connection = new MySqlConnection(fixture.ConnectionString);
    }

    private async Task InitializeSchemaAsync()
    {
        await Connection.ExecuteAsync(@"
            DROP TABLE IF EXISTS products;
            DROP TABLE IF EXISTS product_collections;

            CREATE TABLE IF NOT EXISTS product_collections (
                collection_id INT PRIMARY KEY AUTO_INCREMENT,
                name VARCHAR(255) NOT NULL,
                description TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS products (
                product_id INT PRIMARY KEY AUTO_INCREMENT,
                name VARCHAR(255) NOT NULL,
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
            (await Connection.QueryBySpecAsync(specification, SqlDialect.MySql)).ToList();

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
            await Connection.CountBySpecAsync(specification, SqlDialect.MySql);

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
            await Connection.ExistsBySpecAsync(specification, SqlDialect.MySql);

        // Assert
        results.ShouldBeTrue();
    }
}

