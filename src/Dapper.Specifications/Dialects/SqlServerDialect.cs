namespace Dapper.Specifications.Dialects;

public class SqlServerDialect : ISqlDialect
{
    public string Name => "SQLServer";
    public string ParameterPrefix => "@";

    public string FormatLimitOffset(int skip, int take)
        => $" OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY";

    public string FormatExistsWrapper(string innerQuery)
        => $"SELECT CASE WHEN EXISTS ({innerQuery}) THEN 1 ELSE 0 END";

    public string FormatBoolean(bool value) => value ? "1" : "0";
}