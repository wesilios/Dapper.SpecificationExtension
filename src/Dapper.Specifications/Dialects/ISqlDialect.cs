namespace Dapper.Specifications.Dialects;

public interface ISqlDialect
{
    string Name { get; }

    string FormatLimitOffset(int skip, int take);
    string FormatExistsWrapper(string innerQuery);
    string FormatBoolean(bool value);
    string ParameterPrefix { get; }

    string QuoteIdentifier(string name) => name;
}