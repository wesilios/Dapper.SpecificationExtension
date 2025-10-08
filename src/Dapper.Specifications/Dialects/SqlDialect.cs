namespace Dapper.Specifications.Dialects;

public static class SqlDialect
{
    public static ISqlDialect SqlServer { get; } = new SqlServerDialect();
    public static ISqlDialect PostgreSql { get; } = new PgSqlDialect();
    public static ISqlDialect MySql { get; } = new MySqlDialect();
    public static ISqlDialect Sqlite { get; } = new SqliteDialect();
}