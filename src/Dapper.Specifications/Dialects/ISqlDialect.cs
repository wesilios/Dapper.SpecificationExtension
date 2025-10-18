namespace Dapper.Specifications.Dialects;

/// <summary>
/// Represents a SQL dialect for database-specific query generation.
/// </summary>
public interface ISqlDialect
{
    /// <summary>
    /// Gets the name of the SQL dialect.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Formats the LIMIT/OFFSET clause for pagination.
    /// </summary>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Number of rows to take.</param>
    /// <returns>The formatted LIMIT/OFFSET clause.</returns>
    string FormatLimitOffset(int skip, int take);

    /// <summary>
    /// Wraps a query in an EXISTS clause.
    /// </summary>
    /// <param name="innerQuery">The inner query to wrap.</param>
    /// <returns>The formatted EXISTS query.</returns>
    string FormatExistsWrapper(string innerQuery);

    /// <summary>
    /// Formats a boolean value for the database.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <returns>The formatted boolean value (e.g., "TRUE", "1").</returns>
    string FormatBoolean(bool value);

    /// <summary>
    /// Gets the parameter prefix for the database (e.g., "@", ":", "?").
    /// </summary>
    string ParameterPrefix { get; }

    /// <summary>
    /// Quotes an identifier (table/column name) for the database.
    /// </summary>
    /// <param name="name">The identifier to quote.</param>
    /// <returns>The quoted identifier.</returns>
    string QuoteIdentifier(string name) => name;
}