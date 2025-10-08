using System.Data;
using Dapper.Specifications.Dialects;
using Dapper.Specifications.Evaluators;

namespace Dapper.Specifications.Specifications;

public static class DapperSpecificationExtensions
{
    /// <summary>
    /// Executes a specification query and returns the results.
    /// </summary>
    public static async Task<IEnumerable<T>> QueryBySpecAsync<T>(
        this IDbConnection conn,
        ISpecification<T> spec,
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null)
    {
        dialect ??= SqlDialect.SqlServer;
        return await SpecificationEvaluator.QueryAsync(conn, spec, dialect, dbTransaction);
    }

    /// <summary>
    /// Executes a COUNT(*) query for a specification.
    /// </summary>
    public static async Task<int> CountBySpecAsync<T>(
        this IDbConnection conn,
        ISpecification<T> spec,
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null)
    {
        dialect ??= SqlDialect.SqlServer;
        return await SpecificationEvaluator.CountAsync(conn, spec, dialect, dbTransaction);
    }

    /// <summary>
    /// Executes an EXISTS() query for a specification.
    /// </summary>
    public static async Task<bool> ExistsBySpecAsync<T>(
        this IDbConnection conn,
        ISpecification<T> spec,
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null)
    {
        dialect ??= SqlDialect.SqlServer;
        return await SpecificationEvaluator.ExistsAsync(conn, spec, dialect, dbTransaction);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (JOINs).
    /// </summary>
    public static async Task<IEnumerable<TReturn>> QueryBySpecAsync<T1, T2, TReturn>(
        this IDbConnection conn,
        ISpecification<TReturn> spec,
        Func<T1, T2, TReturn> map,
        string splitOn = "Id",
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null)
    {
        dialect ??= SqlDialect.SqlServer;
        return await SpecificationEvaluator.QueryAsync(conn, spec, dialect, map, splitOn, dbTransaction);
    }
}