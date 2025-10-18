using Dapper.Specifications.Dialects;
using Dapper.Specifications.Evaluators;
using Dapper.Specifications.Specifications;
using Shouldly;
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
        sql.ShouldContain("OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY");
        sql.ShouldContain("FROM Users");
        sql.ShouldContain("WHERE IsActive = 1");
        sql.ShouldContain("ORDER BY Id ASC");
    }

    [Fact]
    public void Build_Generates_PgSqlSql_WithLimitOffset()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.PostgreSql);

        // Assert
        sql.ShouldContain("LIMIT 10 OFFSET 0");
        sql.ShouldContain("FROM Users");
    }

    [Fact]
    public void Build_Generates_MySqlSql_WithLimitOffset()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.MySql);

        // Assert
        sql.ShouldContain("LIMIT 0, 10");
        sql.ShouldContain("FROM Users");
    }

    [Fact]
    public void Build_Generates_SqliteSql_WithLimitOffset()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.Sqlite);

        // Assert
        sql.ShouldContain("LIMIT 10 OFFSET 0");
        sql.ShouldContain("FROM Users");
    }

    [Fact]
    public void Build_WithNullSpec_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() =>
            SpecificationEvaluator.Build<object>(null!, SqlDialect.SqlServer));
        ex.ParamName.ShouldBe("spec");
    }

    [Fact]
    public void Build_WithSelectClause_ShouldIncludeCustomSelect()
    {
        // Arrange
        var spec = new CustomSelectSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldContain("SELECT Id, Name, Email");
        sql.ShouldNotContain("SELECT *");
    }

    [Fact]
    public void Build_WithJoinClause_ShouldIncludeJoin()
    {
        // Arrange
        var spec = new JoinSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldContain("INNER JOIN Orders ON Users.Id = Orders.UserId");
    }

    [Fact]
    public void Build_WithoutWhereClause_ShouldNotIncludeWhere()
    {
        // Arrange
        var spec = new NoWhereSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldNotContain("WHERE");
        sql.ShouldContain("FROM Users");
    }

    [Fact]
    public void Build_WithoutOrderBy_ShouldNotIncludeOrderBy()
    {
        // Arrange
        var spec = new NoOrderBySpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldNotContain("ORDER BY");
    }

    [Fact]
    public void Build_WithoutPagination_ShouldNotIncludeLimitOffset()
    {
        // Arrange
        var spec = new NoPaginationSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldNotContain("OFFSET");
        sql.ShouldNotContain("FETCH NEXT");
    }

    [Fact]
    public void Build_WithIsCount_ShouldGenerateCountQuery()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer, isCount: true);

        // Assert
        sql.ShouldContain("SELECT COUNT(*)");
        sql.ShouldNotContain("ORDER BY");
        sql.ShouldNotContain("OFFSET");
    }

    [Fact]
    public void Build_WithIsExists_ShouldGenerateExistsQuery()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer, isExists: true);

        // Assert
        sql.ShouldContain("SELECT CASE WHEN EXISTS");
        sql.ShouldContain("SELECT 1 FROM Users");
        sql.ShouldNotContain("ORDER BY");
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