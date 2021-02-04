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
            try
            {
                var products = _client.Execute(c => c.product_search(null, null, productReference));
                return Map(products.First());
            }
            catch (SoapHeaderException ex)
            {
                if (ex.Message.Contains("No product"))
                {
                    throw new NotFoundException($"Could not find a product with reference \"{productReference}\"");
                }
                throw;
            }
        }

        public ProductDto GetProductByName(string name)
        {
            try
            {
                var products = _client.Execute(c => c.product_search(name, null, null));
                return Map(products.First());
            }
            catch (SoapHeaderException ex)
            {
                if (ex.Message.Contains("No product"))
                {
                    throw new NotFoundException($"Could not find a product with name \"{name}\"");
                }
                throw;
            }
        }

        public ProductDto GetProductByOptionId(int optionId)
        {
            try
            {
                var option = _client.Execute(c =>
                    c.item_get(optionId, null, 0, true, false)
                );
                return GetProductById(option.productOption.product.id);
            }
            catch (SoapHeaderException ex)
            {
                if (ex.Message.Contains("No product"))
                {
                    throw new NotFoundException($"Could not find a product for option ID \"{optionId}\"");
                }
                throw;
            }
        }

        public ProductDto[] GetProductsByCategory(int productCategory, int pageSize, int pageIndex, bool availability = true)
        {
            var products = _client.Execute(c =>
                c.product_by_category_list(productCategory, availability, null, null, pageSize * pageIndex, pageSize)
            );
            return products.Select(Map).ToArray();
        }

        public ProductOptionDto[] GetProductOptions(int productId)
        {
            var options = _client.Execute(c => c.product_items(productId));
            return options.Select(Map).ToArray();
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

        public ProductStockDto GetStockLevel(int productId, int optionId)
        {
            return GetStockLevel(productId)
                .FirstOrDefault(x => x.OptionId == optionId);
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

        public void ReserveStock(int optionId, int amount, string reason = null)
        {
            var result = _client.Execute(c => c.stock_reserve(new[]
            {
                new ctStockReserveItemDetails
                {
                    itemId = optionId,
                    locationId = _location.id,
                    qty = amount,
                    reasonText = reason ?? "Reserved by website",
                    updateType = "inc"
                }
            }));
            if (!result.success)
            {
                var errors = string.Join(", ", result.errors.Select(e => e.error));
                throw new InvalidOperationException($"Reserve stock failed with {result.errors.Length} errors ({errors})");
            }
        }

        private static ProductStockDto Map(ctStockLevel stock)
        {
            return new ProductStockDto
            {
                LocationId = stock.locationId,
                OptionId = stock.stkItemId,
                Stock = (int) stock.stock,
                Reserved = (int) stock.reserved
            };
        }

        private static ProductDto Map(ctProduct product)
        {
            return new ProductDto
            {
                Id = product.id,
                Name = product.name
            };
        }

        private static ProductOptionDto Map(ctProductOptionDetails option)
        {
            return new ProductOptionDto
            {
                ProductId = option.productOption.product.id,
                Id = option.productOption.id,
                Name = option.productOption.name
            };
        }
    }
}
