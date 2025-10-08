using Dapper.Specifications.Dialects;
using Dapper.Specifications.Evaluators;
using Dapper.Specifications.Specifications;
using Xunit;

namespace Dapper.Specifications.UnitTests;

public class SpecificationEvaluatorTests
{
    [Fact]
    public void Build_Generates_SqlServerSql_WithPagination()
    {
        var spec = new TestSpecification();
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.SqlServer);

        Assert.Contains("OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY", sql);
        Assert.Contains("FROM Users", sql);
    }

    [Fact]
    public void Build_Generates_PgSqlSql_WithLimitOffset()
    {
        var spec = new TestSpecification();
        var (sql, _) = SpecificationEvaluator.Build(spec, SqlDialect.PostgreSql);

        Assert.Contains("LIMIT 10 OFFSET 0", sql);
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
}