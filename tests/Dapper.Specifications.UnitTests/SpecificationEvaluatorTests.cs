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

    #region DISTINCT Tests

    [Fact]
    public void Build_WithDistinct_ShouldIncludeDistinctKeyword()
    {
        // Arrange
        var spec = new TestSpecification();
        spec.SetDistinct();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldContain("SELECT DISTINCT");
    }

    #endregion

    #region UNION Tests

    [Fact]
    public void Build_WithUnion_ShouldGenerateUnionQuery()
    {
        // Arrange
        var spec1 = new TestSpecification();
        var spec2 = new TestSpecification();
        spec2.AddWhere("IsActive = 0");
        spec1.AddUnion(spec2);

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec1, SqlDialect.SqlServer);

        // Assert
        sql.ShouldContain("UNION");
        sql.ShouldNotContain("UNION ALL");
        sql.ShouldContain("IsActive = 1");
        sql.ShouldContain("IsActive = 0");
    }

    [Fact]
    public void Build_WithUnionAll_ShouldGenerateUnionAllQuery()
    {
        // Arrange
        var spec1 = new TestSpecification();
        var spec2 = new TestSpecification();
        spec2.AddWhere("IsActive = 0");
        spec1.AddUnionAll(spec2);

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec1, SqlDialect.SqlServer);

        // Assert
        sql.ShouldContain("UNION ALL");
    }

    #endregion

    #region Subquery Tests

    [Fact]
    public void Build_WithFromSubquery_ShouldGenerateSubqueryInFromClause()
    {
        // Arrange
        var subquery = new TestSpecification();
        subquery.SetSelectClause("Id, Name");

        var spec = new TestSpecification();
        spec.SetFromSubquery(subquery, "sub");
        spec.AddWhere("sub.Id > 10");

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldContain("FROM (");
        sql.ShouldContain(") sub");
        sql.ShouldContain("sub.Id > 10");
    }

    #endregion

    #region CTE Tests

    [Fact]
    public void Build_WithCTE_ShouldGenerateWithClause()
    {
        // Arrange
        var cteSpec = new TestSpecification();
        cteSpec.SetSelectClause("Id, Name");

        var spec = new TestSpecification();
        spec.AddCommonTableExpression("MyCTE", cteSpec);

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldContain("WITH MyCTE AS (");
        sql.ShouldContain(")");
    }

    [Fact]
    public void Build_WithMultipleCTEs_ShouldGenerateMultipleWithClauses()
    {
        // Arrange
        var cte1 = new TestSpecification();
        cte1.SetSelectClause("Id");

        var cte2 = new TestSpecification();
        cte2.SetSelectClause("Name");

        var spec = new TestSpecification();
        spec.AddCommonTableExpression("CTE1", cte1);
        spec.AddCommonTableExpression("CTE2", cte2);

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldContain("WITH CTE1 AS (");
        sql.ShouldContain(", CTE2 AS (");
    }

    #endregion
}