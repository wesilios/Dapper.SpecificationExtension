namespace Dapper.Specifications.Specifications;

/// <summary>
/// Base class for implementing specifications using the Specification Pattern.
/// Inherit from this class to create reusable, testable query specifications.
/// </summary>
/// <typeparam name="T">The entity type that this specification queries.</typeparam>
/// <example>
/// <code>
/// public class ProductSpecification : Specification&lt;Product&gt;
/// {
///     public ProductSpecification()
///     {
///         TableName = "products";
///         PrimaryKey = "product_id";
///     }
/// }
///
/// var spec = new ProductSpecification()
///     .AddWhere("price > @MinPrice", new { MinPrice = 100 })
///     .AddOrder("price DESC")
///     .SetPaging(0, 10);
/// </code>
/// </example>
public abstract class Specification<T> : ISpecification<T>
{
    /// <inheritdoc />
    public string TableName { get; protected set; } = string.Empty;

    /// <inheritdoc />
    public string PrimaryKey { get; protected set; } = string.Empty;

    /// <inheritdoc />
    public string SelectClause { get; protected set; } = string.Empty;

    /// <inheritdoc />
    public string JoinClause { get; protected set; } = string.Empty;

    /// <inheritdoc />
    public string WhereClause { get; protected set; } = string.Empty;

    /// <inheritdoc />
    public string GroupBy { get; protected set; } = string.Empty;

    /// <inheritdoc />
    public string Having { get; protected set; } = string.Empty;

    /// <inheritdoc />
    public string OrderBy { get; protected set; } = string.Empty;

    /// <inheritdoc />
    public int? Skip { get; protected set; }

    /// <inheritdoc />
    public int? Take { get; protected set; }

    /// <inheritdoc />
    public DynamicParameters Parameters { get; protected init; } = new() { };

    /// <inheritdoc />
    public bool IsDistinct { get; protected set; }

    private readonly List<(ISpecification<T> Spec, bool IsUnionAll)> _unionSpecifications = new();

    /// <inheritdoc />
    public IReadOnlyList<(ISpecification<T> Spec, bool IsUnionAll)> UnionSpecifications =>
        _unionSpecifications.AsReadOnly();

    /// <inheritdoc />
    public ISpecification<T>? FromSubquery { get; protected set; }

    /// <inheritdoc />
    public string? FromSubqueryAlias { get; protected set; }

    private readonly List<(string Name, ISpecification<T> Spec)> _commonTableExpressions = new();

    /// <inheritdoc />
    public IReadOnlyList<(string Name, ISpecification<T> Spec)> CommonTableExpressions =>
        _commonTableExpressions.AsReadOnly();

    /// <inheritdoc />
    public ISpecification<T> SetSelectClause(string clause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clause);
        SelectClause = clause;
        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> SetDistinct()
    {
        IsDistinct = true;
        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> AddWhere(string clause, object? parameters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clause);

        if (string.IsNullOrEmpty(WhereClause))
            WhereClause = clause;
        else
            WhereClause += $" AND {clause}";

        if (parameters != null)
            MergeParameters(Parameters, parameters);

        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> AddWhereOr(string clause, object? parameters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clause);

        if (string.IsNullOrEmpty(WhereClause))
            WhereClause = clause;
        else
            WhereClause += $" OR {clause}";

        if (parameters != null)
            MergeParameters(Parameters, parameters);

        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> AddInnerJoin(string clause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clause);
        JoinClause += AddJoin(InnerJoin, clause);
        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> AddLeftJoin(string clause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clause);
        JoinClause += AddJoin(LeftJoin, clause);
        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> AddRightJoin(string clause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clause);
        JoinClause += AddJoin(RightJoin, clause);
        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> AddFullJoin(string clause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clause);
        JoinClause += AddJoin(FullJoin, clause);
        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> AddCrossJoin(string clause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clause);
        JoinClause += AddJoin(CrossJoin, clause);
        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> AddGroupBy(string clause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clause);
        GroupBy = string.IsNullOrWhiteSpace(GroupBy) ? clause : $"{GroupBy}, {clause}";
        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> AddHaving(string clause, object? parameters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clause);

        if (string.IsNullOrEmpty(Having))
            Having = clause;
        else
            Having += $" AND {clause}";

        if (parameters != null)
            MergeParameters(Parameters, parameters);

        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> AddOrder(string clause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clause);
        OrderBy = string.IsNullOrWhiteSpace(OrderBy) ? clause : $"{OrderBy}, {clause}";
        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> SetPaging(int skip, int take)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);
        Skip = skip;
        Take = take;
        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> AddUnion(ISpecification<T> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        _unionSpecifications.Add((specification, false));
        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> AddUnionAll(ISpecification<T> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        _unionSpecifications.Add((specification, true));
        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> SetFromSubquery(ISpecification<T> subquery, string alias)
    {
        ArgumentNullException.ThrowIfNull(subquery);
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        FromSubquery = subquery;
        FromSubqueryAlias = alias;
        return this;
    }

    /// <inheritdoc />
    public ISpecification<T> AddCommonTableExpression(string name, ISpecification<T> specification)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(specification);
        _commonTableExpressions.Add((name, specification));
        return this;
    }

    private void MergeParameters(DynamicParameters a, object b)
    {
        var parameters = b.GetType().GetProperties()
            .Where(p => p.GetIndexParameters().Length == 0);

        foreach (var prop in parameters)
        {
            a.Add(prop.Name, prop.GetValue(b));
        }
    }

    private string AddJoin(string joinType, string joinClause)
    {
        return $" {joinType} {joinClause}";
    }
}