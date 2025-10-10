using Dapper.Specifications.Dialects;
using FluentAssertions;
using Xunit;

namespace Dapper.Specifications.UnitTests.SqlDialectTests;

public class SqlServerDialectTests
{
    private readonly SqlServerDialect _dialect;

    public SqlServerDialectTests()
    {
        _dialect = new SqlServerDialect();
    }

    [Fact]
    public void Name_ShouldReturn_SQLServer()
    {
        // Act
        var name = _dialect.Name;

        // Assert
        name.Should().Be("SQLServer");
    }

    [Fact]
    public void ParameterPrefix_ShouldReturn_AtSymbol()
    {
        // Act
        var prefix = _dialect.ParameterPrefix;

        // Assert
        prefix.Should().Be("@");
    }

    [Fact]
    public void FormatLimitOffset_ShouldReturn_OffsetFetchNext_Syntax()
    {
        // Arrange
        int skip = 10;
        int take = 20;

        // Act
        var result = _dialect.FormatLimitOffset(skip, take);

        // Assert
        result.Should().Be(" OFFSET 10 ROWS FETCH NEXT 20 ROWS ONLY");
    }

    [Fact]
    public void FormatLimitOffset_WithZeroSkip_ShouldReturn_CorrectSyntax()
    {
        // Arrange
        int skip = 0;
        int take = 50;

        // Act
        var result = _dialect.FormatLimitOffset(skip, take);

        // Assert
        result.Should().Be(" OFFSET 0 ROWS FETCH NEXT 50 ROWS ONLY");
    }

    [Fact]
    public void FormatExistsWrapper_ShouldReturn_CaseWhenExists_Syntax()
    {
        // Arrange
        var innerQuery = "SELECT 1 FROM Users WHERE IsActive = 1";

        // Act
        var result = _dialect.FormatExistsWrapper(innerQuery);

        // Assert
        result.Should().Be("SELECT CASE WHEN EXISTS (SELECT 1 FROM Users WHERE IsActive = 1) THEN 1 ELSE 0 END");
    }

    [Fact]
    public void FormatBoolean_WithTrue_ShouldReturn_One()
    {
        // Act
        var result = _dialect.FormatBoolean(true);

        // Assert
        result.Should().Be("1");
    }

    [Fact]
    public void FormatBoolean_WithFalse_ShouldReturn_Zero()
    {
        // Act
        var result = _dialect.FormatBoolean(false);

        // Assert
        result.Should().Be("0");
    }

    [Fact]
    public void QuoteIdentifier_ShouldReturnUnquotedName()
    {
        // Arrange
        ISqlDialect dialect = _dialect;

        // Act
        var result = dialect.QuoteIdentifier("TableName");

        // Assert
        result.Should().Be("TableName");
    }

}