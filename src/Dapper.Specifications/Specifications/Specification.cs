namespace Dapper.Specifications.Specifications;

public abstract class Specification<T> : ISpecification<T>
{
    public string TableName { get; protected set; } = string.Empty;
    public string PrimaryKey { get; protected set; } = string.Empty;
    public string SelectClause { get; protected set; } = string.Empty;
    public string JoinClause { get; protected set; } = string.Empty;
    public string WhereClause { get; protected set; } = string.Empty;
    public string OrderBy { get; protected set; } = string.Empty;
    public int? Skip { get; protected set; }
    public int? Take { get; protected set; }
    public object Parameters { get; protected set; } = new { };

    // Helper methods for a fluent API
    public void AddWhere(string clause, object? parameters = null)
    {
        if (string.IsNullOrEmpty(WhereClause))
            WhereClause = clause;
        else
            WhereClause += $" AND {clause}";

        if (parameters != null)
            Parameters = MergeParameters(Parameters, parameters);
    }

    public void AddJoin(string clause) =>
        JoinClause += $" {clause}";

    public void AddOrder(string clause) =>
        OrderBy = string.IsNullOrWhiteSpace(OrderBy) ? clause : $"{OrderBy}, {clause}";

    public void SetPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    private object MergeParameters(object a, object b)
    {
        var mergeParameters = a.GetType()
            .GetProperties()
            .Where(p => p.GetIndexParameters().Length == 0) // Skip indexed properties
            .ToDictionary(p => p.Name, p => p.GetValue(a));

        foreach (var prop in b.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0))
        {
            mergeParameters[prop.Name] = prop.GetValue(b);
        }

        return mergeParameters;
    }
}