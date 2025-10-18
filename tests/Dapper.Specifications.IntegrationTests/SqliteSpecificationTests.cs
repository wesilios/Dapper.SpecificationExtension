using Dapper.Specifications.Dialects;
using Dapper.Specifications.IntegrationTests.Fixtures;
using Dapper.Specifications.IntegrationTests.MockSpecifications;
using Dapper.Specifications.Specifications;
using Microsoft.Data.Sqlite;
using Shouldly;

namespace Dapper.Specifications.IntegrationTests;

public class SqliteSpecificationTests : SpecificationIntegrationTestBase<SqliteDatabaseFixture>
{
    public SqliteSpecificationTests(SqliteDatabaseFixture fixture) : base(fixture)
    {
        // SQLite requires a persistent connection for in-memory databases
        Connection = new SqliteConnection(fixture.ConnectionString);
        Connection.Open();
    }

    private async Task InitializeSchemaAsync()
    {
        await Connection.ExecuteAsync(@"
            DROP TABLE IF EXISTS products;
            DROP TABLE IF EXISTS product_collections;

            CREATE TABLE IF NOT EXISTS product_collections (
                collection_id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                description TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS products (
                product_id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                price REAL NOT NULL,
                collection_id INTEGER NOT NULL,
                FOREIGN KEY (collection_id) REFERENCES product_collections(collection_id)
            );
        ");

        await Connection.ExecuteAsync("DELETE FROM products;");
        await Connection.ExecuteAsync("DELETE FROM product_collections;");
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
        var specification = new ProductSpecification(useSqliteAliases: true);

        specification.AddWhere("collection_id = @CollectionId", new { CollectionId = 1 });
        specification.AddWhere("price > @MinPrice", new { MinPrice = 180 });

        // Act
        var results =
            (await Connection.QueryBySpecAsync(specification, SqlDialect.Sqlite)).ToList();

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
        var specification = new ProductSpecification(useSqliteAliases: true);

        specification.AddWhere("collection_id = @CollectionId", new { CollectionId = 1 });
        specification.AddWhere("price > @MinPrice", new { MinPrice = 180 });

        // Act
        var results =
            await Connection.CountBySpecAsync(specification, SqlDialect.Sqlite);

        // Assert
        results.ShouldBe(1);
    }

    [Fact]
    public async Task Product_ExistsBySpecAsync_ShouldReturn_ExpectedResults()
    {
        // Arrange
        await InitializeSchemaAsync();
        var specification = new ProductSpecification(useSqliteAliases: true);

        specification.AddWhere("collection_id = @CollectionId", new { CollectionId = 1 });
        specification.AddWhere("price > @MinPrice", new { MinPrice = 180 });

        // Act
        var results =
            await Connection.ExistsBySpecAsync(specification, SqlDialect.Sqlite);

        // Assert
        results.ShouldBeTrue();
    }
}

