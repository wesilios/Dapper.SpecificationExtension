using Dapper.Specifications.Dialects;
using Dapper.Specifications.Evaluators;
using Dapper.Specifications.Specifications;
using Shouldly;
using Xunit;

namespace Dapper.Specifications.UnitTests;

public class SpecificationEvaluatorIntegrationTests
{
    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 20)]
    [InlineData(100, 50)]
    public void Build_WithDifferentPagination_ShouldGenerateCorrectSql(int skip, int take)
    {
        // Arrange
        var spec = new UserSpecification();
        spec.SetPaging(skip, take);

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldContain($"OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY");
    }

    [Fact]
    public void Build_ComplexQuery_WithAllClauses_ShouldGenerateCorrectSql()
    {
        // Arrange
        var spec = new ComplexUserSpecification();

        // Act
        var (sql, parameters) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldContain("SELECT u.Id, u.Name, u.Email, o.OrderDate");
        sql.ShouldContain("FROM Users u");
        sql.ShouldContain("INNER JOIN Orders o ON u.Id = o.UserId");
        sql.ShouldContain("WHERE u.IsActive = 1 AND u.Age > @MinAge");
        sql.ShouldContain("ORDER BY u.Name ASC, u.CreatedDate DESC");
        sql.ShouldContain("OFFSET 0 ROWS FETCH NEXT 25 ROWS ONLY");
        parameters.ShouldNotBeNull();
    }

    [Fact]
    public void Build_CountQuery_ShouldNotIncludeOrderByOrPagination()
    {
        // Arrange
        var spec = new ComplexUserSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer, isCount: true);

        // Assert
        sql.ShouldContain("SELECT COUNT(*)");
        sql.ShouldContain("FROM Users u");
        sql.ShouldContain("WHERE u.IsActive = 1 AND u.Age > @MinAge");
        sql.ShouldNotContain("ORDER BY");
        sql.ShouldNotContain("OFFSET");
        sql.ShouldNotContain("FETCH NEXT");
    }

    [Fact]
    public void Build_ExistsQuery_ShouldGenerateMinimalQuery()
    {
        // Arrange
        var spec = new ComplexUserSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer, isExists: true);

        // Assert
        sql.ShouldContain("SELECT CASE WHEN EXISTS");
        sql.ShouldContain("SELECT 1 FROM Users u");
        sql.ShouldContain("WHERE u.IsActive = 1 AND u.Age > @MinAge");
        sql.ShouldNotContain("ORDER BY");
        sql.ShouldNotContain("OFFSET");
    }

    [Fact]
    public void Build_WithMultipleJoins_ShouldIncludeAllJoins()
    {
        // Arrange
        var spec = new MultiJoinSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldContain("INNER JOIN Orders o ON u.Id = o.UserId");
        sql.ShouldContain("LEFT JOIN Products p ON o.ProductId = p.Id");
        sql.ShouldContain("LEFT JOIN Categories c ON p.CategoryId = c.Id");
    }

    [Fact]
    public void Build_WithDynamicParameters_ShouldReturnParameters()
    {
        // Arrange
        var spec = new ParameterizedSpecification();

        // Act
        var (sql, parameters) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldContain("WHERE Age > @MinAge AND Age < @MaxAge AND Name LIKE @NamePattern");
        parameters.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("SqlServer")]
    [InlineData("PostgreSQL")]
    [InlineData("MySQL")]
    [InlineData("SQLite")]
    public void Build_WithDifferentDialects_ShouldGenerateDialectSpecificSql(string dialectName)
    {
        // Arrange
        var spec = new UserSpecification();
        spec.SetPaging(10, 20);

        ISqlDialect dialect = dialectName switch
        {
            "SqlServer" => SqlDialect.SqlServer,
            "PostgreSQL" => SqlDialect.PostgreSql,
            "MySQL" => SqlDialect.MySql,
            "SQLite" => SqlDialect.Sqlite,
            _ => SqlDialect.SqlServer
        };

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, dialect);

        // Assert
        sql.ShouldNotBeNullOrEmpty();
        sql.ShouldContain("FROM Users");

        // Verify dialect-specific pagination
        switch (dialectName)
        {
            case "SqlServer":
                sql.ShouldContain("OFFSET 10 ROWS FETCH NEXT 20 ROWS ONLY");
                break;
            case "PostgreSQL":
            case "SQLite":
                sql.ShouldContain("LIMIT 20 OFFSET 10");
                break;
            case "MySQL":
                sql.ShouldContain("LIMIT 10, 20");
                break;
        }
    }

    [Fact]
    public void Build_MinimalSpecification_ShouldGenerateBasicQuery()
    {
        // Arrange
        var spec = new MinimalSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldBe("SELECT * FROM Users");
    }

    [Fact]
    public void Build_WithEmptySelectClause_ShouldDefaultToSelectAll()
    {
        // Arrange
        var spec = new UserSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.ShouldContain("SELECT *");
    }

    private class UserSpecification : Specification<User>
    {
        public UserSpecification()
        {
            TableName = "Users";
            PrimaryKey = "Id";
        }
    }

    private class ComplexUserSpecification : Specification<User>
    {
        public ComplexUserSpecification()
        {
            TableName = "Users u";
            PrimaryKey = "Id";
            SelectClause = "u.Id, u.Name, u.Email, o.OrderDate";
            JoinClause = "INNER JOIN Orders o ON u.Id = o.UserId";
            WhereClause = "u.IsActive = 1 AND u.Age > @MinAge";
            OrderBy = "u.Name ASC, u.CreatedDate DESC";
            Skip = 0;
            Take = 25;
            Parameters = new DynamicParameters(new { MinAge = 18 });
        }
    }

    private class MultiJoinSpecification : Specification<User>
    {
        public MultiJoinSpecification()
        {
            TableName = "Users u";
            PrimaryKey = "Id";
            AddJoin("INNER JOIN Orders o ON u.Id = o.UserId");
            AddJoin("LEFT JOIN Products p ON o.ProductId = p.Id");
            AddJoin("LEFT JOIN Categories c ON p.CategoryId = c.Id");
        }
    }

    private class ParameterizedSpecification : Specification<User>
    {
        public ParameterizedSpecification()
        {
            TableName = "Users";
            PrimaryKey = "Id";
            AddWhere("Age > @MinAge", new { MinAge = 18 });
            AddWhere("Age < @MaxAge", new { MaxAge = 65 });
            AddWhere("Name LIKE @NamePattern", new { NamePattern = "%John%" });
        }
    }

    private class MinimalSpecification : Specification<User>
    {
        public MinimalSpecification()
        {
            TableName = "Users";
            PrimaryKey = "Id";
        }
    }

    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}