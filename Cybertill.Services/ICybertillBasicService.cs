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
        int GetStockLevel(int productId);
        ProductStockDto[] GetStockLevels(int pageSize, int pageIndex, DateTime? updatedSince = null);
        void ReserveStock(int productId, int amount, string reason = null);
    }
}
