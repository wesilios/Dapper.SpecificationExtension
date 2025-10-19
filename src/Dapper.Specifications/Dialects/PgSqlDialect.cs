namespace Dapper.Specifications.Dialects;

public class PgSqlDialect : ISqlDialect
{
    public string Name => "PostgreSQL";
    public string ParameterPrefix => "@"; // Dapper still uses @

    public string FormatLimitOffset(int skip, int take)
        => $" {Limit} {take} {Offset} {skip}";

    public string FormatExistsWrapper(string innerQuery)
        => $"{SelectCaseWhenExists} ({innerQuery}) {ThenOneElseZeroEnd}";

    public string FormatBoolean(bool value) => value ? "TRUE" : "FALSE";

    public string QuoteIdentifier(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return $"\"{name.Replace("\"", "\"\"")}\"";
    }
}