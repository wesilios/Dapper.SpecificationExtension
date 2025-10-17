using Dapper.Specifications.Dialects;
using FluentAssertions;
using Xunit;

namespace Dapper.Specifications.UnitTests.SqlDialectTests;

public class PostgreSqlDialectTests
{
    private readonly PgSqlDialect _dialect;

    public PostgreSqlDialectTests()
    {
        _dialect = new PgSqlDialect();
    }

    [Fact]
    public void Name_ShouldReturn_PostgreSQL()
    {
        // Act
        var name = _dialect.Name;

        // Assert
        name.Should().Be("PostgreSQL");
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
    public void FormatLimitOffset_ShouldReturn_LimitOffset_Syntax()
    {
        // Arrange
        int skip = 10;
        int take = 20;

        // Act
        var result = _dialect.FormatLimitOffset(skip, take);

        // Assert
        result.Should().Be(" LIMIT 20 OFFSET 10");
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
        result.Should().Be(" LIMIT 50 OFFSET 0");
    }

    [Fact]
    public void FormatExistsWrapper_ShouldReturn_CaseWhenExists_WithBoolean()
    {
        // Arrange
        var innerQuery = "SELECT 1 FROM Users WHERE IsActive = 1";

        // Act
        var result = _dialect.FormatExistsWrapper(innerQuery);

        // Assert
        result.Should().Be("SELECT CASE WHEN EXISTS (SELECT 1 FROM Users WHERE IsActive = 1) THEN TRUE ELSE FALSE END");
    }

    [Fact]
    public void FormatBoolean_WithTrue_ShouldReturn_TRUE()
    {
        // Act
        var result = _dialect.FormatBoolean(true);

        // Assert
        result.Should().Be("TRUE");
    }

    [Fact]
    public void FormatBoolean_WithFalse_ShouldReturn_FALSE()
    {
        // Act
        var result = _dialect.FormatBoolean(false);

        // Assert
        result.Should().Be("FALSE");
    }
}