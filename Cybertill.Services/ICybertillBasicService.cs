using System;
using Cybertill.Services.Dtos;

namespace Cybertill.Services
{
    public interface ICybertillBasicService
    {
        void Init();
        ProductDto GetProductById(int productId);
        ProductDto GetProductByReference(string productReference);
        ProductDto[] GetProducts(int pageSize, int pageIndex, bool availability = true);
        ProductDto[] GetProductsByCategory(int productCategory, int pageSize, int pageIndex, bool availability = true);
        ProductStockDto[] GetStockLevel(int productId);
        int GetStockLevel(int productId, int itemId);
        ProductStockDto[] GetStockLevels(int pageSize, int pageIndex, DateTime? updatedSince = null);
        void ReserveStock(int itemId, int amount, string reason = null);
    }
}
