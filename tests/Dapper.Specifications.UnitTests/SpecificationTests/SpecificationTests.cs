using Dapper.Specifications.Specifications;
using Shouldly;
using Xunit;

namespace Dapper.Specifications.UnitTests.SpecificationTests;

public class SpecificationTests
{
    [Fact]
    public void AddWhere_WithEmptyWhereClause_ShouldSetWhereClause()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddWhere("Age > 18");

        // Assert
        spec.WhereClause.ShouldBe("Age > 18");
    }

    [Fact]
    public void AddWhere_WithExistingWhereClause_ShouldAppendWithAnd()
    {
        // Arrange
        var spec = new TestSpecification();
        spec.AddWhere("Age > 18");

        // Act
        spec.AddWhere("IsActive = 1");

        // Assert
        spec.WhereClause.ShouldBe("Age > 18 AND IsActive = 1");
    }

    [Fact]
    public void AddWhere_WithParameters_ShouldMergeParameters()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddWhere("Age > @MinAge", new { MinAge = 18 });

        // Assert
        spec.WhereClause.ShouldBe("Age > @MinAge");
        spec.Parameters.ShouldNotBeNull();
    }

    [Fact]
    public void AddWhere_WithMultipleParameters_ShouldMergeAllParameters()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddWhere("Age > @MinAge", new { MinAge = 18 });
        spec.AddWhere("Name = @Name", new { Name = "John" });

        // Assert
        spec.WhereClause.ShouldBe("Age > @MinAge AND Name = @Name");
    }

    [Fact]
    public void AddJoin_ShouldAppendJoinClause()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddJoin("INNER JOIN Orders ON Users.Id = Orders.UserId");

        // Assert
        spec.JoinClause.ShouldBe(" INNER JOIN Orders ON Users.Id = Orders.UserId");
    }

    [Fact]
    public void AddJoin_WithMultipleCalls_ShouldAppendAllJoins()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddJoin("INNER JOIN Orders ON Users.Id = Orders.UserId");
        spec.AddJoin("LEFT JOIN Products ON Orders.ProductId = Products.Id");

        // Assert
        spec.JoinClause.ShouldBe(" INNER JOIN Orders ON Users.Id = Orders.UserId LEFT JOIN Products ON Orders.ProductId = Products.Id");
    }

    [Fact]
    public void AddOrder_WithEmptyOrderBy_ShouldSetOrderBy()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddOrder("Name ASC");

        // Assert
        spec.OrderBy.ShouldBe("Name ASC");
    }

    [Fact]
    public void AddOrder_WithExistingOrderBy_ShouldAppendWithComma()
    {
        // Arrange
        var spec = new TestSpecification();
        spec.AddOrder("Name ASC");

        // Act
        spec.AddOrder("Age DESC");

        // Assert
        spec.OrderBy.ShouldBe("Name ASC, Age DESC");
    }

    [Fact]
    public void SetPaging_ShouldSetSkipAndTake()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.SetPaging(10, 20);

        // Assert
        spec.Skip.ShouldBe(10);
        spec.Take.ShouldBe(20);
    }

    [Fact]
    public void SetPaging_WithZeroSkip_ShouldSetCorrectly()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.SetPaging(0, 50);

        // Assert
        spec.Skip.ShouldBe(0);
        spec.Take.ShouldBe(50);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var spec = new TestSpecification();

        // Assert
        spec.TableName.ShouldBe("Users");
        spec.PrimaryKey.ShouldBe("Id");
        spec.SelectClause.ShouldBeEmpty();
        spec.JoinClause.ShouldBeEmpty();
        spec.WhereClause.ShouldBeEmpty();
        spec.OrderBy.ShouldBeEmpty();
        spec.Skip.ShouldBeNull();
        spec.Take.ShouldBeNull();
        spec.Parameters.ShouldNotBeNull();
    }

    [Fact]
    public void FluentApi_ShouldAllowChaining()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.SetSelectClause("Id, Name AS Username, Age, IsActive");
        spec.AddWhere("Age > @MinAge", new { MinAge = 18 });
        spec.AddWhere("IsActive = 1");
        spec.AddJoin("INNER JOIN Orders ON Users.Id = Orders.UserId");
        spec.AddOrder("Name ASC");
        spec.AddOrder("Age DESC");
        spec.SetPaging(0, 10);

        // Assert
        spec.SelectClause.ShouldBe("Id, Name AS Username, Age, IsActive");
        spec.WhereClause.ShouldBe("Age > @MinAge AND IsActive = 1");
        spec.JoinClause.ShouldContain("INNER JOIN Orders");
        spec.OrderBy.ShouldBe("Name ASC, Age DESC");
        spec.Skip.ShouldBe(0);
        spec.Take.ShouldBe(10);
    }

    #region AddWhereOr Tests

    [Fact]
    public void AddWhereOr_WithEmptyWhereClause_ShouldSetWhereClause()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddWhereOr("Age > 18");

        // Assert
        spec.WhereClause.ShouldBe("Age > 18");
    }

    [Fact]
    public void AddWhereOr_WithExistingWhereClause_ShouldAppendWithOr()
    {
        // Arrange
        var spec = new TestSpecification();
        spec.AddWhere("Age > 18");

        // Act
        spec.AddWhereOr("IsActive = 1");

        // Assert
        spec.WhereClause.ShouldBe("Age > 18 OR IsActive = 1");
    }

    [Fact]
    public void AddWhereOr_WithParameters_ShouldMergeParameters()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddWhereOr("Age > @MinAge", new { MinAge = 18 });

        // Assert
        spec.WhereClause.ShouldBe("Age > @MinAge");
        spec.Parameters.ShouldNotBeNull();
    }

    [Fact]
    public void AddWhereOr_WithMultipleParameters_ShouldMergeAllParameters()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddWhereOr("Age > @MinAge", new { MinAge = 18 });
        spec.AddWhereOr("Name = @Name", new { Name = "John" });

        // Assert
        spec.WhereClause.ShouldBe("Age > @MinAge OR Name = @Name");
    }

    [Fact]
    public void AddWhereOr_MixedWithAddWhere_ShouldCombineCorrectly()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddWhere("Age > @MinAge", new { MinAge = 18 });
        spec.AddWhereOr("IsActive = @IsActive", new { IsActive = true });
        spec.AddWhere("Country = @Country", new { Country = "USA" });

        // Assert
        spec.WhereClause.ShouldBe("Age > @MinAge OR IsActive = @IsActive AND Country = @Country");
    }

    [Fact]
    public void AddWhereOr_ChainedMultipleTimes_ShouldAppendWithOr()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddWhereOr("Status = 'Active'");
        spec.AddWhereOr("Status = 'Pending'");
        spec.AddWhereOr("Status = 'Review'");

        // Assert
        spec.WhereClause.ShouldBe("Status = 'Active' OR Status = 'Pending' OR Status = 'Review'");
    }

    #endregion

    #region AddGroupBy Tests

    [Fact]
    public void AddGroupBy_WithEmptyGroupBy_ShouldSetGroupBy()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddGroupBy("Category");

        // Assert
        spec.GroupBy.ShouldBe("Category");
    }

    [Fact]
    public void AddGroupBy_WithExistingGroupBy_ShouldAppendWithComma()
    {
        // Arrange
        var spec = new TestSpecification();
        spec.AddGroupBy("Category");

        // Act
        spec.AddGroupBy("Status");

        // Assert
        spec.GroupBy.ShouldBe("Category, Status");
    }

    [Fact]
    public void AddGroupBy_WithMultipleCalls_ShouldAppendAllColumns()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddGroupBy("Category");
        spec.AddGroupBy("Status");
        spec.AddGroupBy("Region");

        // Assert
        spec.GroupBy.ShouldBe("Category, Status, Region");
    }

    [Fact]
    public void AddGroupBy_WithMultipleColumnsInOneCall_ShouldSetCorrectly()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddGroupBy("Category, Status, Region");

        // Assert
        spec.GroupBy.ShouldBe("Category, Status, Region");
    }

    #endregion

    #region AddHaving Tests

    [Fact]
    public void AddHaving_WithEmptyHaving_ShouldSetHaving()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddHaving("COUNT(*) > 5");

        // Assert
        spec.Having.ShouldBe("COUNT(*) > 5");
    }

    [Fact]
    public void AddHaving_WithExistingHaving_ShouldAppendWithAnd()
    {
        // Arrange
        var spec = new TestSpecification();
        spec.AddHaving("COUNT(*) > 5");

        // Act
        spec.AddHaving("AVG(Price) > 100");

        // Assert
        spec.Having.ShouldBe("COUNT(*) > 5 AND AVG(Price) > 100");
    }

    [Fact]
    public void AddHaving_WithParameters_ShouldMergeParameters()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddHaving("COUNT(*) > @MinCount", new { MinCount = 5 });

        // Assert
        spec.Having.ShouldBe("COUNT(*) > @MinCount");
        spec.Parameters.ShouldNotBeNull();
    }

    [Fact]
    public void AddHaving_WithMultipleParameters_ShouldMergeAllParameters()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddHaving("COUNT(*) > @MinCount", new { MinCount = 5 });
        spec.AddHaving("AVG(Price) > @MinAvg", new { MinAvg = 100 });

        // Assert
        spec.Having.ShouldBe("COUNT(*) > @MinCount AND AVG(Price) > @MinAvg");
    }

    [Fact]
    public void AddHaving_WithGroupBy_ShouldWorkTogether()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddGroupBy("Category");
        spec.AddHaving("COUNT(*) > @MinCount", new { MinCount = 5 });

        // Assert
        spec.GroupBy.ShouldBe("Category");
        spec.Having.ShouldBe("COUNT(*) > @MinCount");
    }

    #endregion

    #region SetSelectClause Tests

    [Fact]
    public void SetSelectClause_ShouldSetSelectClause()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.SetSelectClause("Id, Name, Age");

        // Assert
        spec.SelectClause.ShouldBe("Id, Name, Age");
    }

    [Fact]
    public void SetSelectClause_WithAggregates_ShouldSetCorrectly()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.SetSelectClause("Category, COUNT(*) as Total, AVG(Price) as AvgPrice");

        // Assert
        spec.SelectClause.ShouldBe("Category, COUNT(*) as Total, AVG(Price) as AvgPrice");
    }

    [Fact]
    public void SetSelectClause_CalledMultipleTimes_ShouldOverwrite()
    {
        // Arrange
        var spec = new TestSpecification();
        spec.SetSelectClause("Id, Name");

        // Act
        spec.SetSelectClause("Id, Name, Age");

        // Assert
        spec.SelectClause.ShouldBe("Id, Name, Age");
    }

    #endregion

    #region DISTINCT Tests

    [Fact]
    public void SetDistinct_ShouldSetIsDistinctToTrue()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.SetDistinct();

        // Assert
        spec.IsDistinct.ShouldBeTrue();
    }

    [Fact]
    public void SetDistinct_WithSelectClause_ShouldWorkTogether()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.SetDistinct();
        spec.SetSelectClause("Category");

        // Assert
        spec.IsDistinct.ShouldBeTrue();
        spec.SelectClause.ShouldBe("Category");
    }

    #endregion

    #region UNION Tests

    [Fact]
    public void AddUnion_ShouldAddSpecificationToUnionList()
    {
        // Arrange
        var spec1 = new TestSpecification();
        var spec2 = new TestSpecification();

        // Act
        spec1.AddUnion(spec2);

        // Assert
        spec1.UnionSpecifications.Count.ShouldBe(1);
        spec1.UnionSpecifications[0].Spec.ShouldBe(spec2);
        spec1.UnionSpecifications[0].IsUnionAll.ShouldBeFalse();
    }

    [Fact]
    public void AddUnionAll_ShouldAddSpecificationToUnionListWithUnionAllFlag()
    {
        // Arrange
        var spec1 = new TestSpecification();
        var spec2 = new TestSpecification();

        // Act
        spec1.AddUnionAll(spec2);

        // Assert
        spec1.UnionSpecifications.Count.ShouldBe(1);
        spec1.UnionSpecifications[0].Spec.ShouldBe(spec2);
        spec1.UnionSpecifications[0].IsUnionAll.ShouldBeTrue();
    }

    [Fact]
    public void AddUnion_WithMultipleSpecs_ShouldAddAllToList()
    {
        // Arrange
        var spec1 = new TestSpecification();
        var spec2 = new TestSpecification();
        var spec3 = new TestSpecification();

        // Act
        spec1.AddUnion(spec2);
        spec1.AddUnionAll(spec3);

        // Assert
        spec1.UnionSpecifications.Count.ShouldBe(2);
        spec1.UnionSpecifications[0].IsUnionAll.ShouldBeFalse();
        spec1.UnionSpecifications[1].IsUnionAll.ShouldBeTrue();
    }

    #endregion

    #region Subquery Tests

    [Fact]
    public void SetFromSubquery_ShouldSetSubqueryAndAlias()
    {
        // Arrange
        var spec = new TestSpecification();
        var subquery = new TestSpecification();

        // Act
        spec.SetFromSubquery(subquery, "sub");

        // Assert
        spec.FromSubquery.ShouldBe(subquery);
        spec.FromSubqueryAlias.ShouldBe("sub");
    }

    #endregion

    #region CTE Tests

    [Fact]
    public void AddCommonTableExpression_ShouldAddCTEToList()
    {
        // Arrange
        var spec = new TestSpecification();
        var cteSpec = new TestSpecification();

        // Act
        spec.AddCommonTableExpression("MyCTE", cteSpec);

        // Assert
        spec.CommonTableExpressions.Count.ShouldBe(1);
        spec.CommonTableExpressions[0].Name.ShouldBe("MyCTE");
        spec.CommonTableExpressions[0].Spec.ShouldBe(cteSpec);
    }

    [Fact]
    public void AddCommonTableExpression_WithMultipleCTEs_ShouldAddAllToList()
    {
        // Arrange
        var spec = new TestSpecification();
        var cte1 = new TestSpecification();
        var cte2 = new TestSpecification();

        // Act
        spec.AddCommonTableExpression("CTE1", cte1);
        spec.AddCommonTableExpression("CTE2", cte2);

        // Assert
        spec.CommonTableExpressions.Count.ShouldBe(2);
        spec.CommonTableExpressions[0].Name.ShouldBe("CTE1");
        spec.CommonTableExpressions[1].Name.ShouldBe("CTE2");
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
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }
}