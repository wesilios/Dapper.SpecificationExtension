using Dapper.Specifications.Dialects;
using Dapper.Specifications.Evaluators;
using Dapper.Specifications.Specifications;
using FluentAssertions;
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
        sql.Should().Contain($"OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY");
    }

    [Fact]
    public void Build_ComplexQuery_WithAllClauses_ShouldGenerateCorrectSql()
    {
        // Arrange
        var spec = new ComplexUserSpecification();

        // Act
        var (sql, parameters) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.Should().Contain("SELECT u.Id, u.Name, u.Email, o.OrderDate");
        sql.Should().Contain("FROM Users u");
        sql.Should().Contain("INNER JOIN Orders o ON u.Id = o.UserId");
        sql.Should().Contain("WHERE u.IsActive = 1 AND u.Age > @MinAge");
        sql.Should().Contain("ORDER BY u.Name ASC, u.CreatedDate DESC");
        sql.Should().Contain("OFFSET 0 ROWS FETCH NEXT 25 ROWS ONLY");
        parameters.Should().NotBeNull();
    }

    [Fact]
    public void Build_CountQuery_ShouldNotIncludeOrderByOrPagination()
    {
        // Arrange
        var spec = new ComplexUserSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer, isCount: true);

        // Assert
        sql.Should().Contain("SELECT COUNT(*)");
        sql.Should().Contain("FROM Users u");
        sql.Should().Contain("WHERE u.IsActive = 1 AND u.Age > @MinAge");
        sql.Should().NotContain("ORDER BY");
        sql.Should().NotContain("OFFSET");
        sql.Should().NotContain("FETCH NEXT");
    }

    [Fact]
    public void Build_ExistsQuery_ShouldGenerateMinimalQuery()
    {
        // Arrange
        var spec = new ComplexUserSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer, isExists: true);

        // Assert
        sql.Should().Contain("SELECT CASE WHEN EXISTS");
        sql.Should().Contain("SELECT 1 FROM Users u");
        sql.Should().Contain("WHERE u.IsActive = 1 AND u.Age > @MinAge");
        sql.Should().NotContain("ORDER BY");
        sql.Should().NotContain("OFFSET");
    }

    [Fact]
    public void Build_WithMultipleJoins_ShouldIncludeAllJoins()
    {
        // Arrange
        var spec = new MultiJoinSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.Should().Contain("INNER JOIN Orders o ON u.Id = o.UserId");
        sql.Should().Contain("LEFT JOIN Products p ON o.ProductId = p.Id");
        sql.Should().Contain("LEFT JOIN Categories c ON p.CategoryId = c.Id");
    }

    [Fact]
    public void Build_WithDynamicParameters_ShouldReturnParameters()
    {
        // Arrange
        var spec = new ParameterizedSpecification();

        // Act
        var (sql, parameters) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.Should().Contain("WHERE Age > @MinAge AND Age < @MaxAge AND Name LIKE @NamePattern");
        parameters.Should().NotBeNull();
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
        sql.Should().NotBeNullOrEmpty();
        sql.Should().Contain("FROM Users");
        
        // Verify dialect-specific pagination
        switch (dialectName)
        {
            case "SqlServer":
                sql.Should().Contain("OFFSET 10 ROWS FETCH NEXT 20 ROWS ONLY");
                break;
            case "PostgreSQL":
            case "SQLite":
                sql.Should().Contain("LIMIT 20 OFFSET 10");
                break;
            case "MySQL":
                sql.Should().Contain("LIMIT 10, 20");
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
        sql.Should().Be("SELECT * FROM Users");
    }

    [Fact]
    public void Build_WithEmptySelectClause_ShouldDefaultToSelectAll()
    {
        // Arrange
        var spec = new UserSpecification();

        // Act
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        // Assert
        sql.Should().Contain("SELECT *");
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
            Parameters = new { MinAge = 18 };
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

