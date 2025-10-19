namespace Dapper.Specifications.Dialects;

public class MySqlDialect : ISqlDialect
{
    public string Name => "MySQL";
    public string ParameterPrefix => "@";

    public string FormatLimitOffset(int skip, int take)
        => $" {Limit} {skip}{Comma} {take}";

    public string FormatExistsWrapper(string innerQuery)
        => $"{SelectExists} ({innerQuery})";

    public string FormatBoolean(bool value) => value ? "1" : "0";

    public string QuoteIdentifier(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return $"`{name.Replace("`", "``")}`";
    }
}