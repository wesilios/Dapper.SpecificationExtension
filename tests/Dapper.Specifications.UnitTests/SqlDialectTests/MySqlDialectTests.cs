using Dapper.Specifications.Dialects;
using Shouldly;
using Xunit;

namespace Dapper.Specifications.UnitTests.SqlDialectTests;

public class MySqlDialectTests
{
    private readonly MySqlDialect _dialect;

    public MySqlDialectTests()
    {
        _dialect = new MySqlDialect();
    }

    [Fact]
    public void Name_ShouldReturn_MySQL()
    {
        // Act
        var name = _dialect.Name;

        // Assert
        name.ShouldBe("MySQL");
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
    public void FormatLimitOffset_ShouldReturn_LimitWithComma_Syntax()
    {
        // Arrange
        int skip = 10;
        int take = 20;

        // Act
        var result = _dialect.FormatLimitOffset(skip, take);

        // Assert
        result.ShouldBe(" LIMIT 10, 20");
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
        result.ShouldBe(" LIMIT 0, 50");
    }

    [Fact]
    public void FormatExistsWrapper_ShouldReturn_SelectExists_Syntax()
    {
        // Arrange
        var innerQuery = "SELECT 1 FROM Users WHERE IsActive = 1";

        // Act
        var result = _dialect.FormatExistsWrapper(innerQuery);

        // Assert
        result.ShouldBe("SELECT EXISTS (SELECT 1 FROM Users WHERE IsActive = 1)");
    }

    [Fact]
    public void FormatBoolean_WithTrue_ShouldReturn_One()
    {
        // Act
        var result = _dialect.FormatBoolean(true);

        // Assert
        result.ShouldBe("1");
    }

    [Fact]
    public void FormatBoolean_WithFalse_ShouldReturn_Zero()
    {
        // Act
        var result = _dialect.FormatBoolean(false);

        // Assert
        result.ShouldBe("0");
    }

    [Fact]
    public void QuoteIdentifier_ShouldReturnQuotedName()
    {
        // Act
        var result = _dialect.QuoteIdentifier("TableName");

        // Assert
        result.ShouldBe("`TableName`");
    }

    [Fact]
    public void QuoteIdentifier_WithBacktick_ShouldEscapeBacktick()
    {
        // Act
        var result = _dialect.QuoteIdentifier("Table`Name");

        // Assert
        result.ShouldBe("`Table``Name`");
    }

    [Fact]
    public void QuoteIdentifier_WithNull_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => _dialect.QuoteIdentifier(null!));
        ex.ParamName.ShouldBe("name");
    }

    [Fact]
    public void QuoteIdentifier_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => _dialect.QuoteIdentifier(""));
        ex.ParamName.ShouldBe("name");
    }
}