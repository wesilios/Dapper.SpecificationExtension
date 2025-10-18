namespace Dapper.Specifications.Specifications;

/// <summary>
/// Represents a specification for building dynamic database queries using the Specification Pattern.
/// </summary>
/// <typeparam name="T">The entity type that this specification queries.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the primary key column name for the table.
    /// </summary>
    string PrimaryKey { get; }

    /// <summary>
    /// Gets the table name to query.
    /// </summary>
    string TableName { get; }

    /// <summary>
    /// Gets the SELECT clause. If empty, defaults to "*".
    /// </summary>
    string SelectClause { get; }

    /// <summary>
    /// Gets the JOIN clause(s) for the query.
    /// </summary>
    string JoinClause { get; }

    /// <summary>
    /// Gets the WHERE clause with AND/OR conditions.
    /// </summary>
    string WhereClause { get; }

    /// <summary>
    /// Gets the GROUP BY clause for aggregate queries.
    /// </summary>
    string GroupBy { get; }

    /// <summary>
    /// Gets the HAVING clause for filtering grouped results.
    /// </summary>
    string Having { get; }

    /// <summary>
    /// Gets the ORDER BY clause for sorting results.
    /// </summary>
    string OrderBy { get; }

    /// <summary>
    /// Gets the number of rows to skip for pagination.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Gets the number of rows to take for pagination.
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Gets the dynamic parameters for the query.
    /// </summary>
    DynamicParameters Parameters { get; }

    /// <summary>
    /// Gets whether to use DISTINCT in the SELECT clause.
    /// </summary>
    bool IsDistinct { get; }

    /// <summary>
    /// Gets the list of specifications to UNION with this specification.
    /// </summary>
    IReadOnlyList<(ISpecification<T> Spec, bool IsUnionAll)> UnionSpecifications { get; }

    /// <summary>
    /// Gets the subquery specification for FROM clause (if any).
    /// </summary>
    ISpecification<T>? FromSubquery { get; }

    /// <summary>
    /// Gets the alias for the FROM subquery.
    /// </summary>
    string? FromSubqueryAlias { get; }

    /// <summary>
    /// Gets the list of Common Table Expressions (CTEs).
    /// </summary>
    IReadOnlyList<(string Name, ISpecification<T> Spec)> CommonTableExpressions { get; }

    /// <summary>
    /// Sets a custom SELECT clause.
    /// </summary>
    /// <param name="clause">The SELECT clause (e.g., "id, name, price").</param>
    /// <returns>The specification instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when clause is null, empty, or whitespace.</exception>
    ISpecification<T> SetSelectClause(string clause);

    /// <summary>
    /// Enables DISTINCT to return only unique rows.
    /// </summary>
    /// <returns>The specification instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// spec.SetDistinct()
    ///     .SetSelectClause("category")
    ///     .AddWhere("active = @Active", new { Active = true });
    /// // Generates: SELECT DISTINCT category FROM products WHERE active = @Active
    /// </code>
    /// </example>
    ISpecification<T> SetDistinct();

    /// <summary>
    /// Adds a WHERE condition using AND logic.
    /// </summary>
    /// <param name="clause">The WHERE condition (e.g., "price > @MinPrice").</param>
    /// <param name="parameters">Optional parameters for the condition.</param>
    /// <returns>The specification instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when clause is null, empty, or whitespace.</exception>
    /// <example>
    /// <code>
    /// spec.AddWhere("price > @MinPrice", new { MinPrice = 100 })
    ///     .AddWhere("category = @Category", new { Category = "Electronics" });
    /// // Generates: WHERE price > @MinPrice AND category = @Category
    /// </code>
    /// </example>
    ISpecification<T> AddWhere(string clause, object? parameters = null);

    /// <summary>
    /// Adds a WHERE condition using OR logic.
    /// </summary>
    /// <param name="clause">The WHERE condition (e.g., "featured = @Featured").</param>
    /// <param name="parameters">Optional parameters for the condition.</param>
    /// <returns>The specification instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when clause is null, empty, or whitespace.</exception>
    /// <example>
    /// <code>
    /// spec.AddWhere("price > @MinPrice", new { MinPrice = 1000 })
    ///     .AddWhereOr("category = @Category", new { Category = "Premium" });
    /// // Generates: WHERE price > @MinPrice OR category = @Category
    /// </code>
    /// </example>
    ISpecification<T> AddWhereOr(string clause, object? parameters = null);

    /// <summary>
    /// Adds a JOIN clause to the query.
    /// </summary>
    /// <param name="clause">The JOIN clause (e.g., "INNER JOIN categories c ON p.category_id = c.id").</param>
    /// <returns>The specification instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when clause is null, empty, or whitespace.</exception>
    ISpecification<T> AddJoin(string clause);

    /// <summary>
    /// Adds a GROUP BY clause for aggregate queries.
    /// </summary>
    /// <param name="clause">The GROUP BY columns (e.g., "category, supplier_id").</param>
    /// <returns>The specification instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when clause is null, empty, or whitespace.</exception>
    ISpecification<T> AddGroupBy(string clause);

    /// <summary>
    /// Adds a HAVING condition for filtering grouped results using AND logic.
    /// </summary>
    /// <param name="clause">The HAVING condition (e.g., "COUNT(*) > @MinCount").</param>
    /// <param name="parameters">Optional parameters for the condition.</param>
    /// <returns>The specification instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when clause is null, empty, or whitespace.</exception>
    ISpecification<T> AddHaving(string clause, object? parameters = null);

    /// <summary>
    /// Adds an ORDER BY clause for sorting results.
    /// </summary>
    /// <param name="clause">The ORDER BY clause (e.g., "price DESC, name ASC").</param>
    /// <returns>The specification instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when clause is null, empty, or whitespace.</exception>
    ISpecification<T> AddOrder(string clause);

    /// <summary>
    /// Sets pagination parameters for the query.
    /// </summary>
    /// <param name="skip">Number of rows to skip (must be >= 0).</param>
    /// <param name="take">Number of rows to take (must be > 0).</param>
    /// <returns>The specification instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when skip is negative or take is zero or negative.</exception>
    ISpecification<T> SetPaging(int skip, int take);

    /// <summary>
    /// Adds a UNION with another specification.
    /// </summary>
    /// <param name="specification">The specification to UNION with.</param>
    /// <returns>The specification instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when specification is null.</exception>
    /// <example>
    /// <code>
    /// var spec1 = new ProductSpecification()
    ///     .AddWhere("category = @Category", new { Category = "Electronics" });
    /// var spec2 = new ProductSpecification()
    ///     .AddWhere("category = @Category", new { Category = "Books" });
    /// spec1.AddUnion(spec2);
    /// // Generates: SELECT * FROM products WHERE category = 'Electronics' UNION SELECT * FROM products WHERE category = 'Books'
    /// </code>
    /// </example>
    ISpecification<T> AddUnion(ISpecification<T> specification);

    /// <summary>
    /// Adds a UNION ALL with another specification.
    /// </summary>
    /// <param name="specification">The specification to UNION ALL with.</param>
    /// <returns>The specification instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when specification is null.</exception>
    /// <example>
    /// <code>
    /// var spec1 = new ProductSpecification()
    ///     .AddWhere("price > @Price", new { Price = 100 });
    /// var spec2 = new ProductSpecification()
    ///     .AddWhere("featured = @Featured", new { Featured = true });
    /// spec1.AddUnionAll(spec2);
    /// // Generates: SELECT * FROM products WHERE price > 100 UNION ALL SELECT * FROM products WHERE featured = true
    /// </code>
    /// </example>
    ISpecification<T> AddUnionAll(ISpecification<T> specification);

    /// <summary>
    /// Sets a subquery as the FROM clause source.
    /// </summary>
    /// <param name="subquery">The specification to use as a subquery.</param>
    /// <param name="alias">The alias for the subquery.</param>
    /// <returns>The specification instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when subquery is null.</exception>
    /// <exception cref="ArgumentException">Thrown when alias is null, empty, or whitespace.</exception>
    /// <example>
    /// <code>
    /// var subquery = new ProductSpecification()
    ///     .SetSelectClause("category, AVG(price) as avg_price")
    ///     .AddGroupBy("category");
    /// var spec = new ProductSpecification()
    ///     .SetFromSubquery(subquery, "sub")
    ///     .AddWhere("sub.avg_price > @MinAvg", new { MinAvg = 100 });
    /// // Generates: SELECT * FROM (SELECT category, AVG(price) as avg_price FROM products GROUP BY category) sub WHERE sub.avg_price > 100
    /// </code>
    /// </example>
    ISpecification<T> SetFromSubquery(ISpecification<T> subquery, string alias);

    /// <summary>
    /// Adds a Common Table Expression (CTE) to the query.
    /// </summary>
    /// <param name="name">The name of the CTE.</param>
    /// <param name="specification">The specification that defines the CTE.</param>
    /// <returns>The specification instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when specification is null.</exception>
    /// <example>
    /// <code>
    /// var cteSpec = new ProductSpecification()
    ///     .SetSelectClause("category, AVG(price) as avg_price")
    ///     .AddGroupBy("category");
    /// var spec = new ProductSpecification()
    ///     .AddCommonTableExpression("CategoryAvg", cteSpec)
    ///     .SetTableName("CategoryAvg")
    ///     .AddWhere("avg_price > @MinAvg", new { MinAvg = 100 });
    /// // Generates: WITH CategoryAvg AS (SELECT category, AVG(price) as avg_price FROM products GROUP BY category) SELECT * FROM CategoryAvg WHERE avg_price > 100
    /// </code>
    /// </example>
    ISpecification<T> AddCommonTableExpression(string name, ISpecification<T> specification);
}