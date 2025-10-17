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