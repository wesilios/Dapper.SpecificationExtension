using Dapper.Specification.Abstractions;

namespace Dapper.Specification;

public class Specification<T> : ISpecification<T>
{
    public string TableName { get; protected set; } = string.Empty;
    public string SelectClause { get; protected set; } = string.Empty;
    public string JoinClause { get; protected set; } = string.Empty;
    public string WhereClause { get; protected set; } = string.Empty;
    public string OrderBy { get; protected set; } = string.Empty;
    public int? Skip { get; protected set; }
    public int? Take { get; protected set; }
    public object Parameters { get; protected set; } = new { };
    
    // Helper methods for a fluent API
    protected void AddWhere(string clause, object parameters = null)
    {
        if (string.IsNullOrEmpty(WhereClause))
            WhereClause = clause;
        else
            WhereClause += $" AND {clause}";

        if (parameters != null)
            Parameters = MergeParameters(Parameters, parameters);
    }

    protected void AddJoin(string clause) =>
        JoinClause += $" {clause}";

    protected void AddOrder(string clause) =>
        OrderBy = string.IsNullOrWhiteSpace(OrderBy) ? clause : $"{OrderBy}, {clause}";

    protected void SetPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    private object MergeParameters(object a, object b)
    {
        var dictA = a.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(a));
        foreach (var prop in b.GetType().GetProperties())
            dictA[prop.Name] = prop.GetValue(b);
        return dictA;
    }
}