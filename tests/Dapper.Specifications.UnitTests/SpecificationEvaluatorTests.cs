using Dapper.Specifications.Dialects;
using Dapper.Specifications.Evaluators;
using Dapper.Specifications.Specifications;
using FluentAssertions;
using Xunit;

namespace Dapper.Specifications.UnitTests;

public class SpecificationEvaluatorTests
{
    [Fact]
    public void Build_Generates_SqlServerSql_WithPagination()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.Should().Contain("OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY");
        sql.Should().Contain("FROM Users");
        sql.Should().Contain("WHERE IsActive = 1");
        sql.Should().Contain("ORDER BY Id ASC");
    }

    [Fact]
    public void Build_Generates_PgSqlSql_WithLimitOffset()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.PostgreSql);

        // Assert
        sql.Should().Contain("LIMIT 10 OFFSET 0");
        sql.Should().Contain("FROM Users");
    }

    [Fact]
    public void Build_Generates_MySqlSql_WithLimitOffset()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.MySql);

        // Assert
        sql.Should().Contain("LIMIT 0, 10");
        sql.Should().Contain("FROM Users");
    }

    [Fact]
    public void Build_Generates_SqliteSql_WithLimitOffset()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.Sqlite);

        // Assert
        sql.Should().Contain("LIMIT 10 OFFSET 0");
        sql.Should().Contain("FROM Users");
    }

    [Fact]
    public void Build_WithNullSpec_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Action act = () => SpecificationEvaluator.Build<object>(null!, SqlDialect.SqlServer);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("spec");
    }

    [Fact]
    public void Build_WithSelectClause_ShouldIncludeCustomSelect()
    {
        // Arrange
        var spec = new CustomSelectSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.Should().Contain("SELECT Id, Name, Email");
        sql.Should().NotContain("SELECT *");
    }

    [Fact]
    public void Build_WithJoinClause_ShouldIncludeJoin()
    {
        // Arrange
        var spec = new JoinSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.Should().Contain("INNER JOIN Orders ON Users.Id = Orders.UserId");
    }

    [Fact]
    public void Build_WithoutWhereClause_ShouldNotIncludeWhere()
    {
        // Arrange
        var spec = new NoWhereSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.Should().NotContain("WHERE");
        sql.Should().Contain("FROM Users");
    }

    [Fact]
    public void Build_WithoutOrderBy_ShouldNotIncludeOrderBy()
    {
        // Arrange
        var spec = new NoOrderBySpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.Should().NotContain("ORDER BY");
    }

    [Fact]
    public void Build_WithoutPagination_ShouldNotIncludeLimitOffset()
    {
        // Arrange
        var spec = new NoPaginationSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.Should().NotContain("OFFSET");
        sql.Should().NotContain("FETCH NEXT");
    }

    [Fact]
    public void Build_WithIsCount_ShouldGenerateCountQuery()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer, isCount: true);

        // Assert
        sql.Should().Contain("SELECT COUNT(*)");
        sql.Should().NotContain("ORDER BY");
        sql.Should().NotContain("OFFSET");
    }

    [Fact]
    public void Build_WithIsExists_ShouldGenerateExistsQuery()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer, isExists: true);

        // Assert
        sql.Should().Contain("SELECT CASE WHEN EXISTS");
        sql.Should().Contain("SELECT 1 FROM Users");
        sql.Should().NotContain("ORDER BY");
    }

    private class TestSpecification : Specification<object>
    {
        public TestSpecification()
        {
            PrimaryKey = "Id";
            TableName = "Users";
            WhereClause = "IsActive = 1";
            OrderBy = "Id ASC";
            Skip = 0;
            Take = 10;
        }
    }

    private class CustomSelectSpecification : Specification<object>
    {
        public CustomSelectSpecification()
        {
            PrimaryKey = "Id";
            TableName = "Users";
            SelectClause = "Id, Name, Email";
        }
    }

    private class JoinSpecification : Specification<object>
    {
        public JoinSpecification()
        {
            PrimaryKey = "Id";
            TableName = "Users";
            JoinClause = "INNER JOIN Orders ON Users.Id = Orders.UserId";
        }
    }

    private class NoWhereSpecification : Specification<object>
    {
        public NoWhereSpecification()
        {
            PrimaryKey = "Id";
            TableName = "Users";
        }
    }

    private class NoOrderBySpecification : Specification<object>
    {
        public NoOrderBySpecification()
        {
            PrimaryKey = "Id";
            TableName = "Users";
            WhereClause = "IsActive = 1";
        }
    }

    private class NoPaginationSpecification : Specification<object>
    {
        public NoPaginationSpecification()
        {
            PrimaryKey = "Id";
            TableName = "Users";
            WhereClause = "IsActive = 1";
            OrderBy = "Id ASC";
        }
    }
}