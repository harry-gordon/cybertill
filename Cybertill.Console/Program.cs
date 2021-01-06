using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Cybertill.API;
using Cybertill.API.Soap;
using Cybertill.Services;
using Cybertill.Services.Dtos;
using Microsoft.Extensions.Configuration;

namespace Cybertill.Console
{
    class Program
    {
        private static ICybertillClient _client;
        private static ICybertillBasicService _service;

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var config = new CybertillConfig();
            configuration.Bind("Cybertill", config);

            _client = new CybertillClient(config);

            _service = new CybertillBasicService(
                _client,
                new CybertillExactNameLocationSelector("1-11 Molendiner Street")
            );

            _service.Init();

            //StockCheck();
            //CategoriesExample();
            UpdateStock();
        }

        /// <summary>
        /// Example of retrieving all the stock entries updated in the last week
        /// </summary>
        static void StockCheck()
        {
            var oneWeekAgo = DateTime.Now.AddDays(-7);

            var allStock = new List<ProductStockDto>();

            var pageIndex = 0;
            ProductStockDto[] stock;
            do
            {
                stock = _service.GetStockLevels(100, pageIndex++, oneWeekAgo);
                allStock.AddRange(stock);
            }
            while(stock.Length != 0);
        }

        /// <summary>
        /// Simple example of reserving stock
        /// </summary>
        static void UpdateStock()
        {
            // Some safe product references that we can test stock updates on            
            var productRefs = new [] {"10011827", "10011824", "10011831"};

            var productRef = productRefs.First();

            var product = _service.GetProductByReference(productRef);

            var productItems = _client.Execute(c => c.product_items(product.Id));

            var item = productItems.First();

            var stock = _service.GetStockLevel(product.Id);

            var result = _client.Execute(c => c.stock_reserve(new[]
            {
                new ctStockReserveItemDetails
                {
                    itemId = item.productOption.id,
                    locationId = stock.First().LocationId,
                    qty = 1,
                    reasonText = "Testing API - Sale",
                    updateType = "set"
                }
            }));

            stock = _service.GetStockLevel(product.Id);

            //_service.ReserveStock(item.productOption.id, 1, "Testing API - Sale");

            //stock = _service.GetStockLevel(product.Id);
        }

        /// <summary>
        /// Poking around the categories API
        /// </summary>
        static void CategoriesExample()
        {
            var unsafeProductRef = "10003465";

            var result = _client.Execute(c => c.product_search(null, null, unsafeProductRef));
            var p1 = result.First();
            var s1 = _client.Execute(c => c.stock_product(p1.id));

            var a = JsonSerializer.Serialize(p1);

            var detailedProduct = _client.Execute(c => c.product_get_with_udf(p1.id, true));

            var b = JsonSerializer.Serialize(detailedProduct);

            var websites = _client.Execute(c => c.website_list());

            var categories = _client.Execute(c => c.category_list());
            var webCategories = _client.Execute(c => c.category_web_list(websites.First().id));
        }
    }
}
