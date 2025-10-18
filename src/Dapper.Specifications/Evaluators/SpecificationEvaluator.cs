using System.Data;
using System.Text;
using Dapper.Specifications.Dialects;
using Dapper.Specifications.Specifications;
using static Dapper.Specifications.SqlKeywords;

namespace Dapper.Specifications.Evaluators;

public class SpecificationEvaluator
{
    /// <summary>
    /// Builds the SQL string and parameters for a specification.
    /// </summary>
    public static (string Sql, object Params) Build<T>(
        ISpecification<T> spec,
        ISqlDialect dialect,
        bool isCount = false,
        bool isExists = false)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(dialect);
        ArgumentException.ThrowIfNullOrWhiteSpace(spec.TableName, nameof(spec.TableName));

        var allParams = new DynamicParameters(spec.Parameters);

        // Handle special query types first (early returns)
        if (isCount)
            return BuildCountQuery(spec, allParams);

        if (isExists)
            return BuildExistsQuery(spec, dialect, allParams);

        // Build standard SELECT query
        return BuildSelectQuery(spec, dialect, allParams);
    }

    /// <summary>
    /// Builds a COUNT(*) query.
    /// </summary>
    private static (string Sql, object Params) BuildCountQuery<T>(ISpecification<T> spec, DynamicParameters allParams)
    {
        var sb = new StringBuilder()
            .Append(Select).Append(' ').Append(CountAll).Append(' ').Append(From).Append(' ')
            .Append(spec.TableName);

        // Include JOINs and WHERE for accurate count
        AppendClauseIfNotEmpty(sb, spec.JoinClause);
        AppendWhereClause(sb, spec.WhereClause);

        return (sb.ToString(), allParams);
    }

    /// <summary>
    /// Builds an EXISTS query.
    /// </summary>
    private static (string Sql, object Params) BuildExistsQuery<T>(ISpecification<T> spec, ISqlDialect dialect, DynamicParameters allParams)
    {
        var inner = new StringBuilder()
            .Append(SelectOne).Append(' ').Append(From).Append(' ')
            .Append(spec.TableName);

        AppendClauseIfNotEmpty(inner, spec.JoinClause);
        AppendWhereClause(inner, spec.WhereClause);

        var wrappedQuery = dialect.FormatExistsWrapper(inner.ToString());
        return (wrappedQuery, allParams);
    }

    /// <summary>
    /// Builds a standard SELECT query with all clauses.
    /// </summary>
    private static (string Sql, object Params) BuildSelectQuery<T>(ISpecification<T> spec, ISqlDialect dialect, DynamicParameters allParams)
    {
        var sb = new StringBuilder();

        // Common Table Expressions (CTEs)
        AppendCommonTableExpressions(sb, spec, dialect, allParams);

        // SELECT clause
        AppendSelectClause(sb, spec);

        // FROM clause
        AppendFromClause(sb, spec, dialect);

        // JOINs, WHERE, GROUP BY, HAVING, ORDER BY
        AppendClauseIfNotEmpty(sb, spec.JoinClause);
        AppendWhereClause(sb, spec.WhereClause);
        AppendGroupByClause(sb, spec.GroupBy);
        AppendHavingClause(sb, spec.Having);

        // Handle UNION or pagination
        if (spec.UnionSpecifications.Count > 0)
        {
            AppendUnionClauses(sb, spec, dialect, allParams);
        }
        else
        {
            AppendOrderByClause(sb, spec.OrderBy);
            AppendPaginationClause(sb, spec, dialect);
        }

        return (sb.ToString(), allParams);
    }

    /// <summary>
    /// Appends Common Table Expressions (CTEs) to the query.
    /// </summary>
    private static void AppendCommonTableExpressions<T>(StringBuilder sb, ISpecification<T> spec, ISqlDialect dialect, DynamicParameters allParams)
    {
        if (spec.CommonTableExpressions.Count == 0)
            return;

        sb.Append("WITH ");
        for (var i = 0; i < spec.CommonTableExpressions.Count; i++)
        {
            if (i > 0)
                sb.Append(", ");

            var (name, cteSpec) = spec.CommonTableExpressions[i];
            var (cteSql, _) = Build(cteSpec, dialect, false, false);
            sb.Append(name).Append(" AS (").Append(cteSql).Append(')');

            // Merge CTE parameters
            foreach (var paramName in cteSpec.Parameters.ParameterNames)
            {
                allParams.Add(paramName, cteSpec.Parameters.Get<object>(paramName));
            }
        }
        sb.Append(' ');
    }

    /// <summary>
    /// Appends the SELECT clause.
    /// </summary>
    private static void AppendSelectClause<T>(StringBuilder sb, ISpecification<T> spec)
    {
        var selectClause = string.IsNullOrWhiteSpace(spec.SelectClause) ? "*" : spec.SelectClause;
        sb.Append(Select).Append(' ');
        if (spec.IsDistinct)
            sb.Append("DISTINCT ");
        sb.Append(selectClause);
    }

    /// <summary>
    /// Appends the FROM clause.
    /// </summary>
    private static void AppendFromClause<T>(StringBuilder sb, ISpecification<T> spec, ISqlDialect dialect)
    {
        sb.Append(' ').Append(From).Append(' ');

        if (spec.FromSubquery != null && !string.IsNullOrWhiteSpace(spec.FromSubqueryAlias))
        {
            var (subquerySql, _) = Build(spec.FromSubquery, dialect, false, false);
            sb.Append('(').Append(subquerySql).Append(") ").Append(spec.FromSubqueryAlias);
        }
        else
        {
            sb.Append(spec.TableName);
        }
    }

    /// <summary>
    /// Appends a clause to the query if the value is not empty.
    /// </summary>
    private static void AppendClauseIfNotEmpty(StringBuilder sb, string? clause)
    {
        if (!string.IsNullOrWhiteSpace(clause))
            sb.Append(' ').Append(clause);
    }

    /// <summary>
    /// Appends a WHERE clause if the condition is not empty.
    /// </summary>
    private static void AppendWhereClause(StringBuilder sb, string? whereClause)
    {
        if (!string.IsNullOrWhiteSpace(whereClause))
            sb.Append(' ').Append(Where).Append(' ').Append(whereClause);
    }

    /// <summary>
    /// Appends a GROUP BY clause if the grouping is not empty.
    /// </summary>
    private static void AppendGroupByClause(StringBuilder sb, string? groupBy)
    {
        if (!string.IsNullOrWhiteSpace(groupBy))
            sb.Append(' ').Append(GroupBy).Append(' ').Append(groupBy);
    }

    /// <summary>
    /// Appends a HAVING clause if the condition is not empty.
    /// </summary>
    private static void AppendHavingClause(StringBuilder sb, string? having)
    {
        if (!string.IsNullOrWhiteSpace(having))
            sb.Append(' ').Append(Having).Append(' ').Append(having);
    }

    /// <summary>
    /// Appends an ORDER BY clause if the ordering is not empty.
    /// </summary>
    private static void AppendOrderByClause(StringBuilder sb, string? orderBy)
    {
        if (!string.IsNullOrWhiteSpace(orderBy))
            sb.Append(' ').Append(OrderBy).Append(' ').Append(orderBy);
    }

    /// <summary>
    /// Appends pagination (LIMIT/OFFSET) clause if skip and take are specified.
    /// </summary>
    private static void AppendPaginationClause<T>(StringBuilder sb, ISpecification<T> spec, ISqlDialect dialect)
    {
        if (spec is { Skip: not null, Take: not null })
            sb.Append(dialect.FormatLimitOffset(spec.Skip.Value, spec.Take.Value));
    }

    /// <summary>
    /// Appends UNION/UNION ALL clauses with their ORDER BY and pagination.
    /// </summary>
    private static void AppendUnionClauses<T>(StringBuilder sb, ISpecification<T> spec, ISqlDialect dialect, DynamicParameters allParams)
    {
        foreach (var (unionSpec, isUnionAll) in spec.UnionSpecifications)
        {
            sb.Append(isUnionAll ? " UNION ALL " : " UNION ");
            AppendUnionQuery(sb, unionSpec);

            // Merge parameters from union spec
            foreach (var paramName in unionSpec.Parameters.ParameterNames)
            {
                allParams.Add(paramName, unionSpec.Parameters.Get<object>(paramName));
            }
        }

        // Apply ORDER BY and pagination to the entire UNION result
        AppendOrderByClause(sb, spec.OrderBy);
        AppendPaginationClause(sb, spec, dialect);
    }

    /// <summary>
    /// Appends a single UNION query (without ORDER BY and pagination).
    /// </summary>
    private static void AppendUnionQuery<T>(StringBuilder sb, ISpecification<T> unionSpec)
    {
        var selectClause = string.IsNullOrWhiteSpace(unionSpec.SelectClause) ? "*" : unionSpec.SelectClause;
        sb.Append(Select).Append(' ');
        if (unionSpec.IsDistinct)
            sb.Append("DISTINCT ");
        sb.Append(selectClause).Append(' ').Append(From).Append(' ').Append(unionSpec.TableName);

        AppendClauseIfNotEmpty(sb, unionSpec.JoinClause);
        AppendWhereClause(sb, unionSpec.WhereClause);
        AppendGroupByClause(sb, unionSpec.GroupBy);
        AppendHavingClause(sb, unionSpec.Having);
    }

    /// <summary>
    /// Executes a specification-based query and returns results.
    /// </summary>
    public static async Task<IEnumerable<T>> QueryAsync<T>(
        IDbConnection connection,
        ISpecification<T> spec,
        ISqlDialect dialect,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        var (sql, param) = Build(spec, dialect);
        var commandDefinition = new CommandDefinition(sql, param, dbTransaction, cancellationToken: cancellationToken);
        return await connection.QueryAsync<T>(commandDefinition);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (JOIN results).
    /// </summary>
    public static async Task<IEnumerable<TReturn>> QueryAsync<T1, T2, TReturn>(
        IDbConnection connection,
        ISpecification<TReturn> spec,
        ISqlDialect dialect,
        Func<T1, T2, TReturn> map,
        string splitOn,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        var (sql, param) = Build(spec, dialect);
        var commandDefinition = new CommandDefinition(sql, param, dbTransaction, cancellationToken: cancellationToken);
        return await connection.QueryAsync(sql, map, param, dbTransaction, splitOn: splitOn, commandTimeout: commandDefinition.CommandTimeout);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (3 types).
    /// </summary>
    public static async Task<IEnumerable<TReturn>> QueryAsync<T1, T2, T3, TReturn>(
        IDbConnection connection,
        ISpecification<TReturn> spec,
        ISqlDialect dialect,
        Func<T1, T2, T3, TReturn> map,
        string splitOn,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        var (sql, param) = Build(spec, dialect);
        var commandDefinition = new CommandDefinition(sql, param, dbTransaction, cancellationToken: cancellationToken);
        return await connection.QueryAsync(sql, map, param, dbTransaction, splitOn: splitOn, commandTimeout: commandDefinition.CommandTimeout);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (4 types).
    /// </summary>
    public static async Task<IEnumerable<TReturn>> QueryAsync<T1, T2, T3, T4, TReturn>(
        IDbConnection connection,
        ISpecification<TReturn> spec,
        ISqlDialect dialect,
        Func<T1, T2, T3, T4, TReturn> map,
        string splitOn,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        var (sql, param) = Build(spec, dialect);
        var commandDefinition = new CommandDefinition(sql, param, dbTransaction, cancellationToken: cancellationToken);
        return await connection.QueryAsync(sql, map, param, dbTransaction, splitOn: splitOn, commandTimeout: commandDefinition.CommandTimeout);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (5 types).
    /// </summary>
    public static async Task<IEnumerable<TReturn>> QueryAsync<T1, T2, T3, T4, T5, TReturn>(
        IDbConnection connection,
        ISpecification<TReturn> spec,
        ISqlDialect dialect,
        Func<T1, T2, T3, T4, T5, TReturn> map,
        string splitOn,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        var (sql, param) = Build(spec, dialect);
        var commandDefinition = new CommandDefinition(sql, param, dbTransaction, cancellationToken: cancellationToken);
        return await connection.QueryAsync(sql, map, param, dbTransaction, splitOn: splitOn, commandTimeout: commandDefinition.CommandTimeout);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (6 types).
    /// </summary>
    public static async Task<IEnumerable<TReturn>> QueryAsync<T1, T2, T3, T4, T5, T6, TReturn>(
        IDbConnection connection,
        ISpecification<TReturn> spec,
        ISqlDialect dialect,
        Func<T1, T2, T3, T4, T5, T6, TReturn> map,
        string splitOn,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        var (sql, param) = Build(spec, dialect);
        var commandDefinition = new CommandDefinition(sql, param, dbTransaction, cancellationToken: cancellationToken);
        return await connection.QueryAsync(sql, map, param, dbTransaction, splitOn: splitOn, commandTimeout: commandDefinition.CommandTimeout);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (7 types).
    /// </summary>
    public static async Task<IEnumerable<TReturn>> QueryAsync<T1, T2, T3, T4, T5, T6, T7, TReturn>(
        IDbConnection connection,
        ISpecification<TReturn> spec,
        ISqlDialect dialect,
        Func<T1, T2, T3, T4, T5, T6, T7, TReturn> map,
        string splitOn,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        var (sql, param) = Build(spec, dialect);
        var commandDefinition = new CommandDefinition(sql, param, dbTransaction, cancellationToken: cancellationToken);
        return await connection.QueryAsync(sql, map, param, dbTransaction, splitOn: splitOn, commandTimeout: commandDefinition.CommandTimeout);
    }

    /// <summary>
    /// Executes a COUNT(*) query for the given specification.
    /// </summary>
    public static async Task<int> CountAsync<T>(
        IDbConnection connection,
        ISpecification<T> spec,
        ISqlDialect dialect,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        var (sql, param) = Build(spec, dialect, isCount: true);
        var commandDefinition = new CommandDefinition(sql, param, dbTransaction, cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<int>(commandDefinition);
    }

    /// <summary>
    /// Executes an EXISTS() query for the given specification.
    /// </summary>
    public static async Task<bool> ExistsAsync<T>(
        IDbConnection connection,
        ISpecification<T> spec,
        ISqlDialect dialect,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        var (sql, param) = Build(spec, dialect, isExists: true);
        var commandDefinition = new CommandDefinition(sql, param, dbTransaction, cancellationToken: cancellationToken);
        var result = await connection.ExecuteScalarAsync<int>(commandDefinition);
        return result == 1;
    }

    // Synchronous methods

    /// <summary>
    /// Executes a specification-based query and returns results (synchronous).
    /// </summary>
    public static IEnumerable<T> Query<T>(
        IDbConnection connection,
        ISpecification<T> spec,
        ISqlDialect dialect,
        IDbTransaction? dbTransaction = null)
    {
        var (sql, param) = Build(spec, dialect);
        return connection.Query<T>(sql, param, dbTransaction);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (JOIN results) (synchronous).
    /// </summary>
    public static IEnumerable<TReturn> Query<T1, T2, TReturn>(
        IDbConnection connection,
        ISpecification<TReturn> spec,
        ISqlDialect dialect,
        Func<T1, T2, TReturn> map,
        string splitOn,
        IDbTransaction? dbTransaction = null)
    {
        var (sql, param) = Build(spec, dialect);
        return connection.Query(sql, map, param, dbTransaction, splitOn: splitOn);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (3 types) (synchronous).
    /// </summary>
    public static IEnumerable<TReturn> Query<T1, T2, T3, TReturn>(
        IDbConnection connection,
        ISpecification<TReturn> spec,
        ISqlDialect dialect,
        Func<T1, T2, T3, TReturn> map,
        string splitOn,
        IDbTransaction? dbTransaction = null)
    {
        var (sql, param) = Build(spec, dialect);
        return connection.Query(sql, map, param, dbTransaction, splitOn: splitOn);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (4 types) (synchronous).
    /// </summary>
    public static IEnumerable<TReturn> Query<T1, T2, T3, T4, TReturn>(
        IDbConnection connection,
        ISpecification<TReturn> spec,
        ISqlDialect dialect,
        Func<T1, T2, T3, T4, TReturn> map,
        string splitOn,
        IDbTransaction? dbTransaction = null)
    {
        var (sql, param) = Build(spec, dialect);
        return connection.Query(sql, map, param, dbTransaction, splitOn: splitOn);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (5 types) (synchronous).
    /// </summary>
    public static IEnumerable<TReturn> Query<T1, T2, T3, T4, T5, TReturn>(
        IDbConnection connection,
        ISpecification<TReturn> spec,
        ISqlDialect dialect,
        Func<T1, T2, T3, T4, T5, TReturn> map,
        string splitOn,
        IDbTransaction? dbTransaction = null)
    {
        var (sql, param) = Build(spec, dialect);
        return connection.Query(sql, map, param, dbTransaction, splitOn: splitOn);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (6 types) (synchronous).
    /// </summary>
    public static IEnumerable<TReturn> Query<T1, T2, T3, T4, T5, T6, TReturn>(
        IDbConnection connection,
        ISpecification<TReturn> spec,
        ISqlDialect dialect,
        Func<T1, T2, T3, T4, T5, T6, TReturn> map,
        string splitOn,
        IDbTransaction? dbTransaction = null)
    {
        var (sql, param) = Build(spec, dialect);
        return connection.Query(sql, map, param, dbTransaction, splitOn: splitOn);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (7 types) (synchronous).
    /// </summary>
    public static IEnumerable<TReturn> Query<T1, T2, T3, T4, T5, T6, T7, TReturn>(
        IDbConnection connection,
        ISpecification<TReturn> spec,
        ISqlDialect dialect,
        Func<T1, T2, T3, T4, T5, T6, T7, TReturn> map,
        string splitOn,
        IDbTransaction? dbTransaction = null)
    {
        var (sql, param) = Build(spec, dialect);
        return connection.Query(sql, map, param, dbTransaction, splitOn: splitOn);
    }

    /// <summary>
    /// Executes a COUNT(*) query for the given specification (synchronous).
    /// </summary>
    public static int Count<T>(
        IDbConnection connection,
        ISpecification<T> spec,
        ISqlDialect dialect,
        IDbTransaction? dbTransaction = null)
    {
        var (sql, param) = Build(spec, dialect, isCount: true);
        return connection.ExecuteScalar<int>(sql, param, dbTransaction);
    }

    /// <summary>
    /// Executes an EXISTS() query for the given specification (synchronous).
    /// </summary>
    public static bool Exists<T>(
        IDbConnection connection,
        ISpecification<T> spec,
        ISqlDialect dialect,
        IDbTransaction? dbTransaction = null)
    {
        var (sql, param) = Build(spec, dialect, isExists: true);
        var result = connection.ExecuteScalar<int>(sql, param, dbTransaction);
        return result == 1;
    }
}