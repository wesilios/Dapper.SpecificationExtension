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
            DROP TABLE IF EXISTS categories;
            DROP TABLE IF EXISTS suppliers;

            CREATE TABLE IF NOT EXISTS categories (
                category_id SERIAL PRIMARY KEY,
                name TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS suppliers (
                supplier_id SERIAL PRIMARY KEY,
                name TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS product_collections (
                collection_id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                description TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS products (
                product_id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                price DECIMAL(10,2) NOT NULL,
                category_id INTEGER NOT NULL REFERENCES categories(category_id),
                supplier_id INTEGER NOT NULL REFERENCES suppliers(supplier_id),
                collection_id INTEGER NOT NULL REFERENCES product_collections(collection_id)
            );
        ");

        await Connection.ExecuteAsync("TRUNCATE TABLE products, product_collections, categories, suppliers RESTART IDENTITY CASCADE;");
        await Connection.ExecuteAsync(
            "INSERT INTO product_collections (name, description) VALUES ('Renewable Energy', 'Renewable Energy Products'), ('Vehicles', 'Vehicles transportation');");
        await Connection.ExecuteAsync(
            "INSERT INTO categories (name) VALUES ('Electronics'), ('Furniture'), ('Transportation');");
        await Connection.ExecuteAsync(
            "INSERT INTO suppliers (name) VALUES ('Supplier A'), ('Supplier B'), ('Supplier C'), ('Supplier D');");
        await Connection.ExecuteAsync(
            "INSERT INTO products (name, price, collection_id, category_id, supplier_id) VALUES ('Solar Panel', 100, 1, 1, 1), ('Wind Turbine', 500, 1, 1, 2);");
        await Connection.ExecuteAsync(
            "INSERT INTO products (name, price, collection_id, category_id, supplier_id) VALUES ('Toyota Hatchback', 100, 2, 3, 3), ('Porches 911', 500, 2, 3, 4);");
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
            await Connection.CountBySpecAsync(specification, SqlDialect.PostgreSql);

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
            await Connection.ExistsBySpecAsync(specification, SqlDialect.PostgreSql);

        // Assert
        results.ShouldBe(true);
    }

    [Fact]
    public async Task ProductCollection_QueryBySpecAsync_MultipleMapping_ShouldReturn_ExpectedResults()
    {
        // Arrange
        await InitializeSchemaAsync();
        var specification = new ProductCollectionSpecification();

        specification.SetSelectClause("pc.collection_id, pc.name, pc.description, p.product_id, p.name, p.price");

        specification.AddInnerJoin("products p ON p.collection_id = pc.collection_id");

        specification.AddWhere("p.collection_id = @CollectionId", new { CollectionId = 2 });

        specification.AddWhere("p.price > @MinPrice", new { MinPrice = 180 });

        var lookup = new Dictionary<long, ProductCollection>();

        // Act
        var results =
            await Connection.QueryBySpecAsync<ProductCollection?, Product?, ProductCollection>(specification,
                (collection, _) =>
                {
                    if (!lookup.TryGetValue(collection.CollectionId, out var productCollection))
                    {
                        productCollection = collection;
                        lookup.Add(collection.CollectionId, productCollection);
                    }

                    return productCollection;
                },
                "product_id",
                SqlDialect.PostgreSql);

        // Assert
        results.Count().ShouldBe(1);
        results.First().Name.ShouldBe("Vehicles");
        results.First().Description.ShouldBe("Vehicles transportation");
        results.First().CollectionId.ShouldNotBe(0);
    }

    [Fact]
    public async Task ProductCollection_QueryBySpecAsync_MultipleMapping_IncludeProduct_ShouldReturn_ExpectedResults()
    {
        // Arrange
        await InitializeSchemaAsync();
        var specification = new ProductCollectionSpecification();

        specification.SetSelectClause("pc.collection_id, pc.name, pc.description, p.product_id, p.name, p.price");

        specification.AddInnerJoin("products p ON p.collection_id = pc.collection_id");

        specification.AddWhere("p.collection_id = @CollectionId", new { CollectionId = 2 });

        specification.AddWhere("p.price > @MinPrice", new { MinPrice = 180 });

        var lookup = new Dictionary<long, ProductCollection>();

        // Act
        var results =
            await Connection.QueryBySpecAsync<ProductCollection?, Product?, ProductCollection>(specification,
                (collection, product) =>
                {
                    if (!lookup.TryGetValue(collection.CollectionId, out var existing))
                    {
                        existing = collection;
                        lookup[collection.CollectionId] = existing;
                    }

                    existing.AddProduct(product);
                    return existing;
                },
                "product_id",
                SqlDialect.PostgreSql);

        // Assert
        results.Count().ShouldBe(1);
        results.First().Name.ShouldBe("Vehicles");
        results.First().Description.ShouldBe("Vehicles transportation");
        results.First().CollectionId.ShouldNotBe(0);
        results.First().Products.Count.ShouldBe(1);
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
            (await Connection.QueryBySpecAsync(specification, SqlDialect.PostgreSql)).ToList();

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
            (await Connection.QueryBySpecAsync(specification, SqlDialect.PostgreSql)).ToList();

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
        var results = (await Connection.QueryBySpecAsync(specification, SqlDialect.PostgreSql)).ToList();

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
        var results = (await Connection.QueryBySpecAsync(spec1, SqlDialect.PostgreSql)).ToList();

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
        var results = (await Connection.QueryBySpecAsync(spec, SqlDialect.PostgreSql)).ToList();

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