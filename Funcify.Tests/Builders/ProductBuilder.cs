using Funcify.Models;

namespace Funcify.Tests.Builders;

public class ProductBuilder
{
    private static readonly Random random = new Random();

    private Product _product;

    public ProductBuilder()
    {
        _product = new Product
        {
            Name = Guid.NewGuid().ToString(),
            Price = (decimal)(random.NextDouble() * 100),
            Quantity = random.Next(1, 10)
        };
    }

    public ProductBuilder WithId(string id)
    {
        _product.id = id;
        return this;
    }

    public ProductBuilder WithName(string name)
    {
        _product.Name = name;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _product.Price = price;
        return this;
    }

    public ProductBuilder WithQuantity(int quantity)
    {
        _product.Quantity = quantity;
        return this;
    }

    public Product Build()
    {
        return _product;
    }

    public static List<Product> BuildMany(int count)
    {

        var products = new List<Product>();
        for (int i = 0; i < count; i++)
        {
            products.Add(new ProductBuilder()
                .WithName(Guid.NewGuid().ToString())
                .Build());
        }
        return products;
    }
}