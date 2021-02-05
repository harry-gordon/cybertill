using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cybertill.API;
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

            EposInvestigationExample();
            //StockCheckExample();
            //UpdateStockExample();
        }

        private class CsvRow
        {
            public int Id;
            public string Name;
            public string EposCode;

            public CsvRow(int id, string name, string eposCode)
            {
                Id = id;
                Name = name;
                EposCode = eposCode;
            }

            public override string ToString()
            {
                return $"{Id} / {EposCode} / \"{Name}\"";
            }
        }

        private static readonly CsvRow[] ExampleRows =
        {
            new CsvRow(28726, "Tuffstuff Woodchip Wallpaper Single Roll", "10003465"),
            new CsvRow(28727, "Tuffstuff Woodchip Wallpaper Double Roll", "10003466"),
            new CsvRow(24884, "Anthology 01 Coral Steel Grey Wallpaper", "110761"),
            new CsvRow(24885, "Anthology 01 Coral Midnight Black Wallpaper", "110762"),
            new CsvRow(24148, "Curio Seafern Green Wallpaper", "107/2007"),
            new CsvRow(24548, "Evita Water Silk Sprig Silver Wallpaper", "104754"),
            new CsvRow(26487, "Marblesque Marble Charcoal/Bronze Grey/Silver Wallpaper", "FD42267"),
            new CsvRow(28723, "1000 Grade Lining Paper Double Roll", "10003472"),
            new CsvRow(26494, "Dimensions Floral Pink Wallpaper", "FD42555"),
            // Previously these two entries wouldn't load because of an XML parsing problem
            new CsvRow(28306, "Shard Trellis Grey Rose Wallpaper", "FD42604"),
            new CsvRow(29584, "Milano 9 Hessian Off White Wallpaper", "M95621")
        };

        /// <summary>
        /// 
        /// </summary>
        private static void EposInvestigationExample()
        {
            foreach (var row in ExampleRows)
            {
                try
                {
                    var product = _service.GetProductByReference(row.EposCode);

                    System.Console.WriteLine($"{row} =>\n  {product.Id} / \"{product.Name}\"");

                    var stock = _service.GetStockLevel(product.Id);

                    var options = _service.GetProductOptions(product.Id);
                    foreach (var option in options)
                    {
                        var optionStock = stock.FirstOrDefault(o => o.OptionId == option.Id);

                        System.Console.WriteLine($"  - {option.Id} / {option.Reference} / {option.PriceRrp:C} / {option.PriceWeb:C} / available: {optionStock?.Available} / \"{option.Name}\"");
                    }
                }
                catch (NotFoundException)
                {
                    System.Console.WriteLine($"{row} => (none)");
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
    }
}
