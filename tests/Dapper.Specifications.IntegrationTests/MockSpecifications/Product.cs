namespace Dapper.Specifications.IntegrationTests.MockSpecifications;

public record Product
{
    public long ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public long CollectionId { get; set; }
}

public record ProductCollection
{
    public long CollectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();

    public void AddProduct(Product product)
    {
        Products.Add(product);
    }
}