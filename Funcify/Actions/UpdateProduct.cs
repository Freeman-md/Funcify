using Funcify.Contracts.Services;
using Funcify.Models;
using System;
using System.Threading.Tasks;

namespace Funcify.Actions
{
    public class UpdateProduct
    {
        private readonly ICosmosDBService _cosmosDBService;

        private const string DatabaseName = "Funcify";
        private const string ContainerName = "Products";

        public UpdateProduct(ICosmosDBService cosmosDBService)
        {
            _cosmosDBService = cosmosDBService;
        }

        public async Task<Product> Invoke(Product product)
        {
            ValidateProduct(product);
            return await _cosmosDBService.UpdateItem(DatabaseName, ContainerName, product);
        }

        #region Private Helper Methods

        private void ValidateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrWhiteSpace(product.id))
                throw new ArgumentException("Product ID cannot be null or empty.", nameof(product.id));

            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Product name cannot be null or empty.", nameof(product.Name));

            if (product.Price <= 0)
                throw new ArgumentException("Product price must be greater than zero.", nameof(product.Price));

            if (product.Quantity < 0)
                throw new ArgumentException("Product quantity cannot be negative.", nameof(product.Quantity));
        }

        #endregion
    }
}
