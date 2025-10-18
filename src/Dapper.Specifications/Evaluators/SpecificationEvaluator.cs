using System.Data;
using System.Text;
using Dapper.Specifications.Dialects;
using Dapper.Specifications.Specifications;

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

        var sb = new StringBuilder();
        var allParams = new DynamicParameters(spec.Parameters);

        // Common Table Expressions (CTEs)
        if (!isCount && !isExists && spec.CommonTableExpressions.Count > 0)
        {
            sb.Append("WITH ");
            for (var i = 0; i < spec.CommonTableExpressions.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");

                var (name, cteSpec) = spec.CommonTableExpressions[i];
                var (cteSql, _) = Build(cteSpec, dialect, false, false);
                sb.Append(name).Append(" AS (").Append(cteSql).Append(")");

                // Merge CTE parameters
                foreach (var paramName in cteSpec.Parameters.ParameterNames)
                {
                    allParams.Add(paramName, cteSpec.Parameters.Get<object>(paramName));
                }
            }
            sb.Append(" ");
        }

        // SELECT clause
        if (isCount)
        {
            sb.Append("SELECT COUNT(*) FROM ").Append(spec.TableName);
        }
        else if (isExists)
        {
            // Dialect-specific EXISTS query
            var inner = new StringBuilder()
                .Append("SELECT 1 FROM ")
                .Append(spec.TableName);

            if (!string.IsNullOrWhiteSpace(spec.JoinClause))
                inner.Append(" ").Append(spec.JoinClause);

            if (!string.IsNullOrWhiteSpace(spec.WhereClause))
                inner.Append(" WHERE ").Append(spec.WhereClause);

            sb.Append(dialect.FormatExistsWrapper(inner.ToString()));
            return (sb.ToString(), spec.Parameters);
        }
        else
        {
            var selectClause = string.IsNullOrWhiteSpace(spec.SelectClause) ? "*" : spec.SelectClause;
            sb.Append("SELECT ");
            if (spec.IsDistinct)
                sb.Append("DISTINCT ");
            sb.Append(selectClause);

            // FROM clause - either table or subquery
            if (spec.FromSubquery != null && !string.IsNullOrWhiteSpace(spec.FromSubqueryAlias))
            {
                // Build subquery
                var (subquerySql, _) = Build(spec.FromSubquery, dialect, false, false);
                sb.Append(" FROM (").Append(subquerySql).Append(") ").Append(spec.FromSubqueryAlias);
            }
            else
            {
                sb.Append(" FROM ").Append(spec.TableName);
            }
        }

        // JOINs
        if (!string.IsNullOrWhiteSpace(spec.JoinClause))
            sb.Append(" ").Append(spec.JoinClause);

        // WHERE
        if (!string.IsNullOrWhiteSpace(spec.WhereClause))
            sb.Append(" WHERE ").Append(spec.WhereClause);

        // GROUP BY
        if (!isCount && !isExists && !string.IsNullOrWhiteSpace(spec.GroupBy))
            sb.Append(" GROUP BY ").Append(spec.GroupBy);

        // HAVING
        if (!isCount && !isExists && !string.IsNullOrWhiteSpace(spec.Having))
            sb.Append(" HAVING ").Append(spec.Having);

        // ORDER BY
        if (!isCount && !isExists && !string.IsNullOrWhiteSpace(spec.OrderBy))
            sb.Append(" ORDER BY ").Append(spec.OrderBy);

        // Pagination (only if no UNION)
        if (!isCount && !isExists && spec is { Skip: not null, Take: not null } && spec.UnionSpecifications.Count == 0)
            sb.Append(dialect.FormatLimitOffset(spec.Skip.Value, spec.Take.Value));

        // UNION/UNION ALL
        if (!isCount && !isExists && spec.UnionSpecifications.Count > 0)
        {
            foreach (var (unionSpec, isUnionAll) in spec.UnionSpecifications)
            {
                sb.Append(isUnionAll ? " UNION ALL " : " UNION ");

                // Build the union query (without ORDER BY and pagination)
                var unionSb = new StringBuilder();
                var selectClause = string.IsNullOrWhiteSpace(unionSpec.SelectClause) ? "*" : unionSpec.SelectClause;
                unionSb.Append("SELECT ");
                if (unionSpec.IsDistinct)
                    unionSb.Append("DISTINCT ");
                unionSb.Append(selectClause);
                unionSb.Append(" FROM ").Append(unionSpec.TableName);

                if (!string.IsNullOrWhiteSpace(unionSpec.JoinClause))
                    unionSb.Append(" ").Append(unionSpec.JoinClause);

                if (!string.IsNullOrWhiteSpace(unionSpec.WhereClause))
                    unionSb.Append(" WHERE ").Append(unionSpec.WhereClause);

                if (!string.IsNullOrWhiteSpace(unionSpec.GroupBy))
                    unionSb.Append(" GROUP BY ").Append(unionSpec.GroupBy);

                if (!string.IsNullOrWhiteSpace(unionSpec.Having))
                    unionSb.Append(" HAVING ").Append(unionSpec.Having);

                sb.Append(unionSb);

                // Merge parameters from union spec
                foreach (var paramName in unionSpec.Parameters.ParameterNames)
                {
                    allParams.Add(paramName, unionSpec.Parameters.Get<object>(paramName));
                }
            }

            // Apply ORDER BY and pagination to the entire UNION result
            if (!string.IsNullOrWhiteSpace(spec.OrderBy))
                sb.Append(" ORDER BY ").Append(spec.OrderBy);

            if (spec is { Skip: not null, Take: not null })
                sb.Append(dialect.FormatLimitOffset(spec.Skip.Value, spec.Take.Value));
        }

        return (sb.ToString(), allParams);
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