using System;
using System.Linq;
using Cybertill.API;
using Cybertill.API.Soap;
using Cybertill.Services.Dtos;
using Cybertill.Services.Exceptions;

namespace Cybertill.Services
{
    public class CybertillBasicService : ICybertillBasicService
    {
        private readonly ICybertillClient _client;
        private readonly ICybertillLocationSelector _locationSelector;

        private ctLocation _location;

        public CybertillBasicService(ICybertillClient client, ICybertillLocationSelector locationSelector)
        {
            _client = client;
            _locationSelector = locationSelector;
        }

        public void Init()
        {
            _client.Init();

            // Find the default location
            var locations = _client.Execute(c => c.location_list(true, null, null));
            _location = locations.First(l => _locationSelector.IsLocation(l.name, l.area));
        }

        public ProductDto GetProductById(int productId)
        {
            // TODO: Catch the exception
            var product = _client.Execute(c => c.product_get(productId));
            return Map(product);
        }

        public ProductDto GetProductByReference(string productReference)
        {
            var products = _client.Execute(c => c.product_search(null, null, productReference));
            if (!products.Any())
            {
                throw new NotFoundException($"Could not find a product with reference \"{productReference}\"");
            }
            return Map(products.First());
        }

        public ProductDto[] GetProductsByCategory(int productCategory, int pageSize, int pageIndex, bool availability = true)
        {
            var products = _client.Execute(c =>
                c.product_by_category_list(productCategory, availability, null, null, pageSize * pageIndex, pageSize)
            );
            return products.Select(Map).ToArray();
        }

        public ProductDto[] GetProducts(int pageSize, int pageIndex, bool availability = true)
        {
            var products = _client.Execute(c =>
                c.product_list(availability, null, null, pageSize * pageIndex, pageSize)
            );
            return products.Select(Map).ToArray();
        }

        public int GetStockLevel(int productId)
        {
            // TODO: Catch the exception
            var result = _client.Execute(c => c.stock_product_by_location(productId, _location.id));
            var stock = result.FirstOrDefault(x => x.locationId == _location.id);
            return stock == null ? 0 : (int) stock.stock;
        }

        public ProductStockDto[] GetStockLevels(int pageSize, int pageIndex, DateTime? updatedSince = null)
        {
            var stockList = _client.Execute(c =>
                c.stock_list(null, updatedSince?.ToString("yyyy-MM-dd"), pageSize * pageIndex, pageSize)
            );
            return stockList.Select(Map).ToArray();
        }

        public void ReserveStock(int productId, int amount, string reason)
        {
            _client.Execute(c => c.stock_reserve(new[]
            {
                new ctStockReserveItemDetails
                {
                    itemId = productId,
                    locationId = _location.id,
                    qty = 1,
                    reasonText = reason ?? "Reserved by website",
                    updateType = "dec"
                }
            }));
        }

        private ProductStockDto Map(ctStockLevel stock)
        {
            return new ProductStockDto
            {
                LocationId = stock.locationId,
                ProductId = stock.stkItemId,
                Stock = (int) stock.stock
            };
        }

        private ProductDto Map(ctProduct product)
        {
            return new ProductDto
            {
                Id = product.id,
                Name = product.name
            };
        }
    }
}
