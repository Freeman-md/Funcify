using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Funcify.Models;

public class Product
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public required decimal Price { get; set; } = 0;
    public required int Quantity { get; set; } = 0;

    public string Category { get; set; } = "products";
    public string? FileName { get; set; }
    public string? UnprocessedImageUrl { get; set; }
    public string? ProcessedImageUrl { get; set; }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

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
        return $"Product(Id: {id}, Name: {Name}, Price: {Price}, Quantity: {Quantity}, UnprocessedImageUrl: {UnprocessedImageUrl}, ProcessedImageUrl: {ProcessedImageUrl})";
    }
}
