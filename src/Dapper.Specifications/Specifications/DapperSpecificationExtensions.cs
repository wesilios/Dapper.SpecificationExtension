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
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        dialect ??= SqlDialect.SqlServer;
        return await SpecificationEvaluator.QueryAsync(conn, spec, dialect, dbTransaction, cancellationToken);
    }

    /// <summary>
    /// Executes a COUNT(*) query for a specification.
    /// </summary>
    public static async Task<int> CountBySpecAsync<T>(
        this IDbConnection conn,
        ISpecification<T> spec,
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        dialect ??= SqlDialect.SqlServer;
        return await SpecificationEvaluator.CountAsync(conn, spec, dialect, dbTransaction, cancellationToken);
    }

    /// <summary>
    /// Executes an EXISTS() query for a specification.
    /// </summary>
    public static async Task<bool> ExistsBySpecAsync<T>(
        this IDbConnection conn,
        ISpecification<T> spec,
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        dialect ??= SqlDialect.SqlServer;
        return await SpecificationEvaluator.ExistsAsync(conn, spec, dialect, dbTransaction, cancellationToken);
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
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        dialect ??= SqlDialect.SqlServer;
        return await SpecificationEvaluator.QueryAsync(conn, spec, dialect, map, splitOn, dbTransaction, cancellationToken);
    }

    // Synchronous methods

    /// <summary>
    /// Executes a specification query and returns the results (synchronous).
    /// </summary>
    public static IEnumerable<T> QueryBySpec<T>(
        this IDbConnection conn,
        ISpecification<T> spec,
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null)
    {
        dialect ??= SqlDialect.SqlServer;
        return SpecificationEvaluator.Query(conn, spec, dialect, dbTransaction);
    }

    /// <summary>
    /// Executes a COUNT(*) query for a specification (synchronous).
    /// </summary>
    public static int CountBySpec<T>(
        this IDbConnection conn,
        ISpecification<T> spec,
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null)
    {
        dialect ??= SqlDialect.SqlServer;
        return SpecificationEvaluator.Count(conn, spec, dialect, dbTransaction);
    }

    /// <summary>
    /// Executes an EXISTS() query for a specification (synchronous).
    /// </summary>
    public static bool ExistsBySpec<T>(
        this IDbConnection conn,
        ISpecification<T> spec,
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null)
    {
        dialect ??= SqlDialect.SqlServer;
        return SpecificationEvaluator.Exists(conn, spec, dialect, dbTransaction);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (JOINs) (synchronous).
    /// </summary>
    public static IEnumerable<TReturn> QueryBySpec<T1, T2, TReturn>(
        this IDbConnection conn,
        ISpecification<TReturn> spec,
        Func<T1, T2, TReturn> map,
        string splitOn = "Id",
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null)
    {
        dialect ??= SqlDialect.SqlServer;
        return SpecificationEvaluator.Query(conn, spec, dialect, map, splitOn, dbTransaction);
    }

    // Multi-mapping overloads (3-7 types) - Async

    /// <summary>
    /// Executes a specification-based multi-mapping query (3 types).
    /// </summary>
    public static async Task<IEnumerable<TReturn>> QueryBySpecAsync<T1, T2, T3, TReturn>(
        this IDbConnection conn,
        ISpecification<TReturn> spec,
        Func<T1, T2, T3, TReturn> map,
        string splitOn = "Id",
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        dialect ??= SqlDialect.SqlServer;
        return await SpecificationEvaluator.QueryAsync(conn, spec, dialect, map, splitOn, dbTransaction, cancellationToken);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (4 types).
    /// </summary>
    public static async Task<IEnumerable<TReturn>> QueryBySpecAsync<T1, T2, T3, T4, TReturn>(
        this IDbConnection conn,
        ISpecification<TReturn> spec,
        Func<T1, T2, T3, T4, TReturn> map,
        string splitOn = "Id",
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        dialect ??= SqlDialect.SqlServer;
        return await SpecificationEvaluator.QueryAsync(conn, spec, dialect, map, splitOn, dbTransaction, cancellationToken);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (5 types).
    /// </summary>
    public static async Task<IEnumerable<TReturn>> QueryBySpecAsync<T1, T2, T3, T4, T5, TReturn>(
        this IDbConnection conn,
        ISpecification<TReturn> spec,
        Func<T1, T2, T3, T4, T5, TReturn> map,
        string splitOn = "Id",
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        dialect ??= SqlDialect.SqlServer;
        return await SpecificationEvaluator.QueryAsync(conn, spec, dialect, map, splitOn, dbTransaction, cancellationToken);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (6 types).
    /// </summary>
    public static async Task<IEnumerable<TReturn>> QueryBySpecAsync<T1, T2, T3, T4, T5, T6, TReturn>(
        this IDbConnection conn,
        ISpecification<TReturn> spec,
        Func<T1, T2, T3, T4, T5, T6, TReturn> map,
        string splitOn = "Id",
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        dialect ??= SqlDialect.SqlServer;
        return await SpecificationEvaluator.QueryAsync(conn, spec, dialect, map, splitOn, dbTransaction, cancellationToken);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (7 types).
    /// </summary>
    public static async Task<IEnumerable<TReturn>> QueryBySpecAsync<T1, T2, T3, T4, T5, T6, T7, TReturn>(
        this IDbConnection conn,
        ISpecification<TReturn> spec,
        Func<T1, T2, T3, T4, T5, T6, T7, TReturn> map,
        string splitOn = "Id",
        ISqlDialect? dialect = null,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        dialect ??= SqlDialect.SqlServer;
        return await SpecificationEvaluator.QueryAsync(conn, spec, dialect, map, splitOn, dbTransaction, cancellationToken);
    }
}