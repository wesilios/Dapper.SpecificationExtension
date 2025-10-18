namespace Dapper.Specifications.Dialects;

public class PgSqlDialect : ISqlDialect
{
    public string Name => "PostgreSQL";
    public string ParameterPrefix => "@"; // Dapper still uses @

    public string FormatLimitOffset(int skip, int take)
        => $" LIMIT {take} OFFSET {skip}";

    public string FormatExistsWrapper(string innerQuery)
        => $"SELECT CASE WHEN EXISTS ({innerQuery}) THEN 1 ELSE 0 END";

    public string FormatBoolean(bool value) => value ? "TRUE" : "FALSE";

    public string QuoteIdentifier(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return $"\"{name.Replace("\"", "\"\"")}\"";
    }
}