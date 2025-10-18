namespace Dapper.Specifications.Dialects;

/// <summary>
/// Provides static instances of SQL dialects for different databases.
/// </summary>
public static class SqlDialect
{
    /// <summary>
    /// Gets the SQL Server dialect.
    /// </summary>
    public static ISqlDialect SqlServer { get; } = new SqlServerDialect();

    /// <summary>
    /// Gets the PostgreSQL dialect.
    /// </summary>
    public static ISqlDialect PostgreSql { get; } = new PgSqlDialect();

    /// <summary>
    /// Gets the MySQL dialect.
    /// </summary>
    public static ISqlDialect MySql { get; } = new MySqlDialect();

    /// <summary>
    /// Gets the SQLite dialect.
    /// </summary>
    public static ISqlDialect Sqlite { get; } = new SqliteDialect();
}