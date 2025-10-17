using Dapper.Specifications.Dialects;
using Dapper.Specifications.IntegrationTests.Fixtures;
using Dapper.Specifications.IntegrationTests.MockSpecifications;
using Dapper.Specifications.Specifications;
using FluentAssertions;
using Npgsql;

namespace Dapper.Specifications.IntegrationTests;

public class PgSqlSpecificationTests : SpecificationIntegrationTestBase<PgSqlDatabaseFixture>
{
    public PgSqlSpecificationTests(PgSqlDatabaseFixture fixture) : base(fixture)
    {
        Connection = new NpgsqlConnection(fixture.ConnectionString);
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    private async Task InitializeSchemaAsync()
    {
        await Connection.ExecuteAsync(@"
            DROP TABLE IF EXISTS products;
            DROP TABLE IF EXISTS product_collections;

            CREATE TABLE IF NOT EXISTS product_collections (
                collection_id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                description TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS products (
                product_id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                price DECIMAL(10,2) NOT NULL,
                collection_id INTEGER NOT NULL REFERENCES product_collections(collection_id)
            );
        ");

        await Connection.ExecuteAsync("TRUNCATE TABLE products, product_collections RESTART IDENTITY CASCADE;");
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
            (await Connection.QueryBySpecAsync(specification, SqlDialect.PostgreSql)).ToList();

        // Assert
        results[0].Name.Should().Be("Wind Turbine");
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
            await Connection.CountBySpecAsync(specification, SqlDialect.PostgreSql);

        // Assert
        results.Should().Be(1);
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
            await Connection.ExistsBySpecAsync(specification, SqlDialect.PostgreSql);

        // Assert
        results.Should().Be(true);
    }

    [Fact]
    public async Task Product_QueryBySpecAsync_MultipleMapping_ShouldReturn_ExpectedResults()
    {
        // Arrange
        await InitializeSchemaAsync();
        var specification = new ProductCollectionSpecification();

        specification.SetSelectClause("pc.collection_id, pc.name, pc.description, p.product_id");

        specification.AddJoin("INNER JOIN products p ON p.collection_id = pc.collection_id");

        specification.AddWhere("p.collection_id = @CollectionId", new { CollectionId = 2 });

        specification.AddWhere("p.price > @MinPrice", new { MinPrice = 180 });

        var collectionDictionary = new Dictionary<long, ProductCollection>();

        // Act
        var results =
            await Connection.QueryBySpecAsync<ProductCollection?, Product?, ProductCollection>(specification,
                (collection, _) =>
                {
                    if (!collectionDictionary.TryGetValue(collection.CollectionId, out var productCollection))
                    {
                        productCollection = collection;
                        collectionDictionary.Add(collection.CollectionId, productCollection);
                    }

                    return productCollection;
                },
                "product_id",
                SqlDialect.PostgreSql);

        // Assert
        results.Count().Should().Be(1);
        results.First().Name.Should().Be("Vehicles");
        results.First().CollectionId.Should().NotBe(0);
    }
}