using System;
using System.Linq;
using System.Threading.Tasks;
using Cybertill.API;
using Microsoft.Extensions.Configuration;

namespace Cybertill.Console
{
    class Program
    {
        private static ICybertillClient _client;

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

            await _client.InitAsync();

            StockCheck();
        }

        static void StockCheck()
        {
            var oneWeekAgo = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");

            // Assuming we are doing bulk stock updates we can retrieve only new stock updates
            var stockListCount = _client.Execute(c => c.stock_list_count(null, oneWeekAgo));
            var stockList = _client.Execute(c => c.stock_list(null, oneWeekAgo, 0, 100));

            // In this test lets find a product with stock
            var stockEntry = stockList.First(x => x.stock > 10);

            var productId = stockEntry.stkItemId;

            // Just checking our assumption about stkItemId being the product ID
            var productStockEntry = _client.Execute(c => c.stock_product(productId));
            var product = _client.Execute(c => c.product_get(productId));
        }
    }
}
