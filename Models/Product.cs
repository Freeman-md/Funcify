using System;
using System.Text.Json;

public class Product
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required int Quantity { get; set; }
    public string? UnprocessedImageUrl { get; set; }
    public string? ProcessedImageUrl { get; set; }

    public Product()
    {
        Id = Guid.NewGuid();
    }

    // Serialize this Product to JSON
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

    // Deserialize JSON to a Product
    public static Product FromJson(string json)
    {
        return JsonSerializer.Deserialize<Product>(json)!;
    }

    public void AddUnprocessedImageUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
        UnprocessedImageUrl = url;
    }

    public void AddProcessedImageUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
        ProcessedImageUrl = url;
    }

    public override string ToString()
    {
        return $"Product(Id: {Id}, Name: {Name}, Price: {Price}, Quantity: {Quantity}, UnprocessedImageUrl: {UnprocessedImageUrl}, ProcessedImageUrl: {ProcessedImageUrl})";
    }
}
