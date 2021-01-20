using System;
using Cybertill.Services.Dtos;

namespace Cybertill.Services
{
    public interface ICybertillBasicService
    {
        void Init();
        ProductDto GetProductById(int productId);
        ProductDto GetProductByReference(string productReference);
        ProductDto GetProductByOptionId(int optionId);
        ProductDto[] GetProducts(int pageSize, int pageIndex, bool availability = true);
        ProductDto[] GetProductsByCategory(int productCategory, int pageSize, int pageIndex, bool availability = true);
        ProductOptionDto[] GetProductOptions(int productId);
        ProductStockDto[] GetStockLevel(int productId);
        ProductStockDto GetStockLevel(int productId, int optionId);
        ProductStockDto[] GetStockLevels(int pageSize, int pageIndex, DateTime? updatedSince = null);
        void ReserveStock(int optionId, int amount, string reason = null);
    }
}
