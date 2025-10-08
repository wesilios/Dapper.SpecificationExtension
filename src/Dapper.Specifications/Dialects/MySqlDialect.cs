namespace Dapper.Specifications.Dialects;

public class MySqlDialect : ISqlDialect
{
    public string Name => "MySQL";
    public string ParameterPrefix => "@";

    public string FormatLimitOffset(int skip, int take)
        => $" LIMIT {skip}, {take}";

    public string FormatExistsWrapper(string innerQuery)
        => $"SELECT EXISTS ({innerQuery})";

    public string FormatBoolean(bool value) => value ? "1" : "0";
}