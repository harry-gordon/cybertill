using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Cybertill.API;
using Cybertill.API.Soap;
using Cybertill.Services;
using Cybertill.Services.Dtos;
using Cybertill.Services.Exceptions;
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

            //EposCodeExample();
            SkuExample();
            //StockCheckExample();
            //UpdateStockExample();
            //CategoriesExample();
        }

        private class CsvRow
        {
            public int Id;
            public string Name;
            public string EposCode;
            public string Sku;

            public CsvRow(int id, string name, string eposCode, string sku)
            {
                Id = id;
                Name = name;
                EposCode = eposCode;
                Sku = sku;
            }

            public override string ToString()
            {
                return $"{Id} / {EposCode} / {Sku} / \"{Name}\"";
            }
        }

        static CsvRow[] ExampleRows =
        {
            new CsvRow(28726, "Tuffstuff Woodchip Wallpaper Single Roll", "10003465", "5060075555121"),
            new CsvRow(28727, "Tuffstuff Woodchip Wallpaper Double Roll", "10003466", "5060075555985"),
            new CsvRow(24884, "Anthology 01 Coral Steel Grey Wallpaper", "110761", null),
            new CsvRow(24885, "Anthology 01 Coral Midnight Black Wallpaper", "110762", null),
            new CsvRow(24148, "Curio Seafern Green Wallpaper", "107/2007", null),
            new CsvRow(24548, "Evita Water Silk Sprig Silver Wallpaper", "104754", "5011580000000"),
            new CsvRow(26487, "Marblesque Marble Charcoal/Bronze Grey/Silver Wallpaper", "FD42267", "5011420000000"),
            new CsvRow(28723, "1000 Grade Lining Paper Double Roll", "10003472", "5025137213081"),
        };

        /// <summary>
        /// 
        /// </summary>
        private static void EposCodeExample()
        {
            foreach (var row in ExampleRows)
            {
                try
                {
                    var product = _service.GetProductByReference(row.EposCode);
                    System.Console.WriteLine($"{row} => EPOS as Reference => {product.Name}");
                }
                catch (NotFoundException)
                {
                    try
                    {
                        var product = _service.GetProductByReference(row.Id.ToString());
                        System.Console.WriteLine($"{row} => ID as Reference => {product.Name}");
                    }
                    catch (NotFoundException)
                    {
                        try
                        {
                            var product = _service.GetProductByOptionId(row.Id);
                            System.Console.WriteLine($"{row} => ID as Item ID => {product.Name}");
                        }
                        catch (NotFoundException)
                        {
                            try
                            {
                                var product = _service.GetProductByName(row.Name);
                                System.Console.WriteLine($"{row} => Name => {product.Name}");
                            }
                            catch (NotFoundException)
                            {
                                System.Console.WriteLine($"{row} => not found");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void SkuExample()
        {
            foreach (var row in ExampleRows)
            {
                try
                {
                    var product = _service.GetProductByReference(row.EposCode);

                    var rawProduct = _client.Execute(c => c.product_get(product.Id));

                    System.Console.WriteLine($"{row} =>\n  {product.Id} / \"{product.Name}\"");

                    var options = _client.Execute(c => c.product_items(product.Id));
                    foreach (var option in options)
                    {
                        System.Console.WriteLine($"  - {option.productOption.id} / {option.productOption.@ref} / {option.productOptionPrice.priceRrp:C} / {option.productOptionPrice.priceWeb:C} / \"{option.productOption.name}\"");
                    }
                }
                catch (NotFoundException)
                {
                }
            }
        }

        /// <summary>
        /// Example of retrieving all the stock entries updated in the last week
        /// </summary>
        private static void StockCheckExample()
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
        private static void UpdateStockExample()
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
        private static void CategoriesExample()
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
