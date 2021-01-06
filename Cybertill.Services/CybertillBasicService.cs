using System;
using System.Linq;
using System.Web.Services.Protocols;
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
            try
            {
                var product = _client.Execute(c => c.product_get(productId));
                return Map(product);
            }
            catch (SoapHeaderException)
            {
                // TODO: Ideally we should check the message here 
                throw new NotFoundException($"Could not find a product with ID \"{productId}\"");
            }
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

        public ProductStockDto[] GetStockLevel(int productId)
        {
            // For some reason the location filter on the API does not work?
            var result = _client.Execute(c => c.stock_product_by_location(productId, _location.id));
            return result.Where(x => x.locationId == _location.id).Select(Map).ToArray();
        }

        public int GetStockLevel(int productId, int itemId)
        {
            var itemStock = GetStockLevel(productId).FirstOrDefault(x => x.ItemId == itemId);
            return itemStock?.Stock ?? 0;
        }

        public ProductStockDto[] GetStockLevels(int pageSize, int pageIndex, DateTime? updatedSince = null)
        {
            try
            {
                var stockList = _client.Execute(c =>
                    c.stock_list(null, updatedSince?.ToString("yyyy-MM-dd"), pageSize * pageIndex, pageSize)
                );
                return stockList.Select(Map).ToArray();
            }
            catch (SoapHeaderException ex)
            {
                if (ex.Message.Contains("No stock levels found"))
                {
                    return new ProductStockDto[0];
                }

                throw;
            }
        }

        public void ReserveStock(int itemId, int amount, string reason = null)
        {
            var result = _client.Execute(c => c.stock_reserve(new[]
            {
                new ctStockReserveItemDetails
                {
                    itemId = itemId,
                    locationId = _location.id,
                    qty = 1,
                    reasonText = reason ?? "Reserved by website",
                    updateType = "dec"
                }
            }));
            if (!result.success)
            {
                var errors = string.Join(", ", result.errors.Select(e => e.error));
                throw new InvalidOperationException($"Reserve stock failed with {result.errors.Length} errors ({errors})");
            }
        }

        private ProductStockDto Map(ctStockLevel stock)
        {
            return new ProductStockDto
            {
                LocationId = stock.locationId,
                ItemId = stock.stkItemId,
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
