using Dapper.Specifications.Specifications;
using FluentAssertions;
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
        spec.WhereClause.Should().Be("Age > 18");
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
        spec.WhereClause.Should().Be("Age > 18 AND IsActive = 1");
    }

    [Fact]
    public void AddWhere_WithParameters_ShouldMergeParameters()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddWhere("Age > @MinAge", new { MinAge = 18 });

        // Assert
        spec.WhereClause.Should().Be("Age > @MinAge");
        spec.Parameters.Should().NotBeNull();
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
        spec.WhereClause.Should().Be("Age > @MinAge AND Name = @Name");
    }

    [Fact]
    public void AddJoin_ShouldAppendJoinClause()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddJoin("INNER JOIN Orders ON Users.Id = Orders.UserId");

        // Assert
        spec.JoinClause.Should().Be(" INNER JOIN Orders ON Users.Id = Orders.UserId");
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
        spec.JoinClause.Should()
            .Be(" INNER JOIN Orders ON Users.Id = Orders.UserId LEFT JOIN Products ON Orders.ProductId = Products.Id");
    }

    [Fact]
    public void AddOrder_WithEmptyOrderBy_ShouldSetOrderBy()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.AddOrder("Name ASC");

        // Assert
        spec.OrderBy.Should().Be("Name ASC");
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
        spec.OrderBy.Should().Be("Name ASC, Age DESC");
    }

    [Fact]
    public void SetPaging_ShouldSetSkipAndTake()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.SetPaging(10, 20);

        // Assert
        spec.Skip.Should().Be(10);
        spec.Take.Should().Be(20);
    }

    [Fact]
    public void SetPaging_WithZeroSkip_ShouldSetCorrectly()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act
        spec.SetPaging(0, 50);

        // Assert
        spec.Skip.Should().Be(0);
        spec.Take.Should().Be(50);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var spec = new TestSpecification();

        // Assert
        spec.TableName.Should().Be("Users");
        spec.PrimaryKey.Should().Be("Id");
        spec.SelectClause.Should().BeEmpty();
        spec.JoinClause.Should().BeEmpty();
        spec.WhereClause.Should().BeEmpty();
        spec.OrderBy.Should().BeEmpty();
        spec.Skip.Should().BeNull();
        spec.Take.Should().BeNull();
        spec.Parameters.Should().NotBeNull();
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
        spec.SelectClause.Should().Be("Id, Name AS Username, Age, IsActive");
        spec.WhereClause.Should().Be("Age > @MinAge AND IsActive = 1");
        spec.JoinClause.Should().Contain("INNER JOIN Orders");
        spec.OrderBy.Should().Be("Name ASC, Age DESC");
        spec.Skip.Should().Be(0);
        spec.Take.Should().Be(10);
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