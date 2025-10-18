using Dapper.Specifications.Dialects;
using Shouldly;
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
        name.ShouldBe("PostgreSQL");
    }

    [Fact]
    public void ParameterPrefix_ShouldReturn_AtSymbol()
    {
        // Act
        var prefix = _dialect.ParameterPrefix;

        // Assert
        prefix.ShouldBe("@");
    }

    [Fact]
    public void FormatLimitOffset_ShouldReturn_LimitOffset_Syntax()
    {
        // Arrange
        var skip = 10;
        var take = 20;

        // Act
        var result = _dialect.FormatLimitOffset(skip, take);

        // Assert
        result.ShouldBe(" LIMIT 20 OFFSET 10");
    }

    [Fact]
    public void FormatLimitOffset_WithZeroSkip_ShouldReturn_CorrectSyntax()
    {
        // Arrange
        var skip = 0;
        var take = 50;

        // Act
        var result = _dialect.FormatLimitOffset(skip, take);

        // Assert
        result.ShouldBe(" LIMIT 50 OFFSET 0");
    }

    [Fact]
    public void FormatExistsWrapper_ShouldReturn_CaseWhenExists_WithInteger()
    {
        // Arrange
        var innerQuery = "SELECT 1 FROM Users WHERE IsActive = 1";

        // Act
        var result = _dialect.FormatExistsWrapper(innerQuery);

        // Assert
        result.ShouldBe("SELECT CASE WHEN EXISTS (SELECT 1 FROM Users WHERE IsActive = 1) THEN 1 ELSE 0 END");
    }

    [Fact]
    public void FormatBoolean_WithTrue_ShouldReturn_TRUE()
    {
        // Act
        var result = _dialect.FormatBoolean(true);

        // Assert
        result.ShouldBe("TRUE");
    }

    [Fact]
    public void FormatBoolean_WithFalse_ShouldReturn_FALSE()
    {
        // Act
        var result = _dialect.FormatBoolean(false);

        // Assert
        result.ShouldBe("FALSE");
    }
}