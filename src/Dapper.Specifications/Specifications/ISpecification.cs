namespace Dapper.Specifications.Specifications;

public interface ISpecification<T>
{
    string PrimaryKey { get; }
    string TableName { get; }
    string SelectClause { get; }
    string JoinClause { get; }
    string WhereClause { get; }
    string OrderBy { get; }
    int? Skip { get; }
    int? Take { get; }
    DynamicParameters Parameters { get; }

    void SetSelectClause(string clause);
    void AddWhere(string clause, object? parameters = null);
    void AddJoin(string clause);
    void AddOrder(string clause);
    void SetPaging(int skip, int take);
}