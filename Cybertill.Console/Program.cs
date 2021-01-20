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

            StockCheckExample();
            UpdateStockExample();
            CategoriesExample();
        }

        /// <summary>
        /// Example of retrieving all the stock entries updated in the last week
        /// </summary>
        static void StockCheckExample()
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
        /// Simple example of reserving stock for an product option/item
        /// </summary>
        static void UpdateStockExample()
        {
            // Find a product option
            var stockLevels = _service.GetStockLevels(1, 0, DateTime.Now.AddYears(-1));

            foreach (var productStock in stockLevels)
            {
                var optionId = productStock.OptionId;
                var product = _service.GetProductByOptionId(optionId);

                System.Console.WriteLine($"Product: \"${product.Name}\" with option \"{optionId}\"");
                System.Console.WriteLine($"Stock level: {productStock}");

                _service.ReserveStock(optionId, 1, "Testing API - reserve a product");

                var updatedProductStock = _service.GetStockLevel(product.Id, optionId);

                // Reserved level should have increased by 1 and available is reduced
                System.Console.WriteLine($"Updated stock level: {updatedProductStock}");
            }
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

            var detailedProduct = _client.Execute(c => c.product_get_with_udf(p1.id, true));

            var websites = _client.Execute(c => c.website_list());

            var categories = _client.Execute(c => c.category_list());
            var webCategories = _client.Execute(c => c.category_web_list(websites.First().id));
        }
    }
}
