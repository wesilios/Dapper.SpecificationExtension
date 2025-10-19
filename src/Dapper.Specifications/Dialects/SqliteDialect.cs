namespace Dapper.Specifications.Dialects;

public class SqliteDialect : ISqlDialect
{
    public string Name => "SQLite";
    public string ParameterPrefix => "@";

    public string FormatLimitOffset(int skip, int take)
        => $" {Limit} {take} {Offset} {skip}";

    public string FormatExistsWrapper(string innerQuery)
        => $"{SelectExists} ({innerQuery})";

    public string FormatBoolean(bool value) => value ? "1" : "0";

    public string QuoteIdentifier(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return $"\"{name.Replace("\"", "\"\"")}\"";
    }
}