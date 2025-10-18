namespace Dapper.Specifications.Examples.Models;

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public bool Featured { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ProductWithCategory
{
    public Product Product { get; set; } = null!;
    public Category Category { get; set; } = null!;
}

