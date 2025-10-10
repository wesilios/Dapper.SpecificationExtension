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
        if (spec is null)
            throw new ArgumentNullException(nameof(spec));

        var sb = new StringBuilder();

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
            sb.Append("SELECT ").Append(selectClause);
            sb.Append(" FROM ").Append(spec.TableName);
        }

        // JOINs
        if (!string.IsNullOrWhiteSpace(spec.JoinClause))
            sb.Append(" ").Append(spec.JoinClause);

        // WHERE
        if (!string.IsNullOrWhiteSpace(spec.WhereClause))
            sb.Append(" WHERE ").Append(spec.WhereClause);

        // ORDER BY
        if (!isCount && !isExists && !string.IsNullOrWhiteSpace(spec.OrderBy))
            sb.Append(" ORDER BY ").Append(spec.OrderBy);

        // Pagination
        if (!isCount && !isExists && spec is { Skip: not null, Take: not null })
            sb.Append(dialect.FormatLimitOffset(spec.Skip.Value, spec.Take.Value));

        return (sb.ToString(), spec.Parameters);
    }

    /// <summary>
    /// Executes a specification-based query and returns results.
    /// </summary>
    public static async Task<IEnumerable<T>> QueryAsync<T>(
        IDbConnection conn,
        ISpecification<T> spec,
        ISqlDialect dialect,
        IDbTransaction? dbTransaction = null)
    {
        var (sql, param) = Build(spec, dialect);
        return await conn.QueryAsync<T>(sql, param, dbTransaction);
    }

    /// <summary>
    /// Executes a specification-based multi-mapping query (JOIN results).
    /// </summary>
    public static async Task<IEnumerable<TReturn>> QueryAsync<T1, T2, TReturn>(
        IDbConnection conn,
        ISpecification<TReturn> spec,
        ISqlDialect dialect,
        Func<T1, T2, TReturn> map,
        string splitOn,
        IDbTransaction? dbTransaction = null)
    {
        var (sql, param) = Build(spec, dialect);
        return await conn.QueryAsync(sql, map, param, dbTransaction, splitOn: splitOn);
    }

    /// <summary>
    /// Executes a COUNT(*) query for the given specification.
    /// </summary>
    public static async Task<int> CountAsync<T>(
        IDbConnection conn,
        ISpecification<T> spec,
        ISqlDialect dialect,
        IDbTransaction? dbTransaction = null)
    {
        var (sql, param) = Build(spec, dialect, isCount: true);
        return await conn.ExecuteScalarAsync<int>(sql, param, dbTransaction);
    }

    /// <summary>
    /// Executes an EXISTS() query for the given specification.
    /// </summary>
    public static async Task<bool> ExistsAsync<T>(
        IDbConnection conn,
        ISpecification<T> spec,
        ISqlDialect dialect,
        IDbTransaction? dbTransaction = null)
    {
        var (sql, param) = Build(spec, dialect, isExists: true);
        var result = await conn.ExecuteScalarAsync<int>(sql, param, dbTransaction);
        return result == 1;
    }
}