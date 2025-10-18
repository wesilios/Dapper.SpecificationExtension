using Dapper.Specifications.Dialects;
using Dapper.Specifications.Evaluators;
using Dapper.Specifications.Specifications;
using Shouldly;
using Xunit;

namespace Dapper.Specifications.UnitTests;

public class SpecificationEvaluatorValidationTests
{
    [Fact]
    public void Build_WithNullSpec_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() =>
            SpecificationEvaluator.Build<object>(null!, SqlDialect.SqlServer));
        ex.ParamName.ShouldBe("spec");
    }

    [Fact]
    public void Build_WithNullDialect_ShouldThrowArgumentNullException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() =>
            SpecificationEvaluator.Build(spec, null!));
        ex.ParamName.ShouldBe("dialect");
    }

    [Fact]
    public void Build_WithEmptyTableName_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new EmptyTableNameSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() =>
            SpecificationEvaluator.Build(spec, SqlDialect.SqlServer));
        ex.ParamName.ShouldBe("TableName");
    }

    [Fact]
    public void Build_WithWhitespaceTableName_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new WhitespaceTableNameSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() =>
            SpecificationEvaluator.Build(spec, SqlDialect.SqlServer));
        ex.ParamName.ShouldBe("TableName");
    }

    [Fact]
    public void Build_WithValidSpec_ShouldSucceed()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldNotBeNullOrEmpty();
        sql.ShouldContain("FROM Users");
    }

    private class TestSpecification : Specification<object>
    {
        public TestSpecification()
        {
            TableName = "Users";
            PrimaryKey = "Id";
        }
    }

    private class EmptyTableNameSpecification : Specification<object>
    {
        public EmptyTableNameSpecification()
        {
            TableName = "";
            PrimaryKey = "Id";
        }
    }

    private class WhitespaceTableNameSpecification : Specification<object>
    {
        public WhitespaceTableNameSpecification()
        {
            TableName = "   ";
            PrimaryKey = "Id";
        }
    }
}

