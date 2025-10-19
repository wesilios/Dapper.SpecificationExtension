using Dapper.Specifications.Specifications;
using Shouldly;
using Xunit;

namespace Dapper.Specifications.UnitTests.SpecificationTests;

public class SpecificationValidationTests
{
    [Fact]
    public void SetSelectClause_WithNull_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.SetSelectClause(null!));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void SetSelectClause_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.SetSelectClause(""));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void SetSelectClause_WithWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.SetSelectClause("   "));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddWhere_WithNull_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddWhere(null!));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddWhere_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddWhere(""));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddWhere_WithWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddWhere("   "));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddInnerJoin_WithNull_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddInnerJoin(null!));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddInnerJoin_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddInnerJoin(""));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddInnerJoin_WithWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddInnerJoin("   "));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddOrder_WithNull_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddOrder(null!));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddOrder_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddOrder(""));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddOrder_WithWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddOrder("   "));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void SetPaging_WithNegativeSkip_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => spec.SetPaging(-1, 10));
        ex.ParamName.ShouldBe("skip");
    }

    [Fact]
    public void SetPaging_WithNegativeTake_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => spec.SetPaging(0, -1));
        ex.ParamName.ShouldBe("take");
    }

    [Fact]
    public void SetPaging_WithZeroTake_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => spec.SetPaging(0, 0));
        ex.ParamName.ShouldBe("take");
    }

    [Fact]
    public void SetPaging_WithValidValues_ShouldSucceed()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.SetPaging(0, 10);

        // Assert
        spec.Skip.ShouldBe(0);
        spec.Take.ShouldBe(10);
    }

    [Fact]
    public void SetPaging_WithLargeValues_ShouldSucceed()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.SetPaging(1000, 5000);

        // Assert
        spec.Skip.ShouldBe(1000);
        spec.Take.ShouldBe(5000);
    }

    #region AddWhereOr Validation Tests

    [Fact]
    public void AddWhereOr_WithNull_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddWhereOr(null!));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddWhereOr_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddWhereOr(""));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddWhereOr_WithWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddWhereOr("   "));
        ex.ParamName.ShouldBe("clause");
    }

    #endregion

    #region AddGroupBy Validation Tests

    [Fact]
    public void AddGroupBy_WithNull_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddGroupBy(null!));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddGroupBy_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddGroupBy(""));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddGroupBy_WithWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddGroupBy("   "));
        ex.ParamName.ShouldBe("clause");
    }

    #endregion

    #region AddHaving Validation Tests

    [Fact]
    public void AddHaving_WithNull_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddHaving(null!));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddHaving_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddHaving(""));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddHaving_WithWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddHaving("   "));
        ex.ParamName.ShouldBe("clause");
    }

    #endregion

    #region UNION Validation Tests

    [Fact]
    public void AddUnion_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => spec.AddUnion(null!));
        ex.ParamName.ShouldBe("specification");
    }

    [Fact]
    public void AddUnionAll_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => spec.AddUnionAll(null!));
        ex.ParamName.ShouldBe("specification");
    }

    #endregion

    #region Subquery Validation Tests

    [Fact]
    public void SetFromSubquery_WithNullSubquery_ShouldThrowArgumentNullException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => spec.SetFromSubquery(null!, "alias"));
        ex.ParamName.ShouldBe("subquery");
    }

    [Fact]
    public void SetFromSubquery_WithNullAlias_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();
        var subquery = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.SetFromSubquery(subquery, null!));
        ex.ParamName.ShouldBe("alias");
    }

    [Fact]
    public void SetFromSubquery_WithEmptyAlias_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();
        var subquery = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.SetFromSubquery(subquery, ""));
        ex.ParamName.ShouldBe("alias");
    }

    #endregion

    #region CTE Validation Tests

    [Fact]
    public void AddCommonTableExpression_WithNullName_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();
        var cteSpec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddCommonTableExpression(null!, cteSpec));
        ex.ParamName.ShouldBe("name");
    }

    [Fact]
    public void AddCommonTableExpression_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();
        var cteSpec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddCommonTableExpression("", cteSpec));
        ex.ParamName.ShouldBe("name");
    }

    [Fact]
    public void AddCommonTableExpression_WithNullSpecification_ShouldThrowArgumentNullException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => spec.AddCommonTableExpression("CTE", null!));
        ex.ParamName.ShouldBe("specification");
    }

    #endregion

    private class TestSpecification : Specification<User>
    {
        public TestSpecification()
        {
            TableName = "Users";
            PrimaryKey = "Id";
        }
    }

    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

