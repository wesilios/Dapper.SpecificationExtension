namespace Dapper.Specification.Abstractions;

public interface ISpecification<T>
{
    string TableName { get; }
    string SelectClause { get; }
    string JoinClause { get; }
    string WhereClause { get; }
    string OrderBy { get; }
    int? Skip { get; }
    int? Take { get; }
    object Parameters { get; }
}