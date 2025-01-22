namespace Funcify.Actions;

public class ProductImageProcessingMessage
{
    public string ProductId { get; set; }
    public string UnprocessedImageUrl { get; set; }
    public string FileName { get; set; }

    public ProductImageProcessingMessage(string productId, string unprocessedImageUrl, string fileName)
    {
        ProductId = productId;
        UnprocessedImageUrl = unprocessedImageUrl;
        FileName = fileName;
    }
}
