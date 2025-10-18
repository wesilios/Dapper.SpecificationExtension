using static Dapper.Specifications.SqlKeywords;

namespace Dapper.Specifications.Dialects;

public class SqlServerDialect : ISqlDialect
{
    public string Name => "SQLServer";
    public string ParameterPrefix => "@";

    public string FormatLimitOffset(int skip, int take)
        => $" {Offset} {skip} {Rows} {FetchNext} {take} {Rows} {Only}";

    public string FormatExistsWrapper(string innerQuery)
        => $"{SelectCaseWhenExists} ({innerQuery}) {ThenOneElseZeroEnd}";

    public string FormatBoolean(bool value) => value ? "1" : "0";

    public string QuoteIdentifier(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return $"[{name.Replace("]", "]]")}]";
    }
}