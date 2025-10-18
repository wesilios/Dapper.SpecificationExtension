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
    public void AddJoin_WithNull_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddJoin(null!));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddJoin_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddJoin(""));
        ex.ParamName.ShouldBe("clause");
    }

    [Fact]
    public void AddJoin_WithWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var spec = new TestSpecification();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() => spec.AddJoin("   "));
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

