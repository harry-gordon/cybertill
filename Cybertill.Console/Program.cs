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
            //var categories = _client.Execute(c => c.category_list());

            var countries = _client.Execute(c => c.country_list());

            var countryId = countries.First(c => c.iso3166Cc == "GBR").id;

            var locations = _client.Execute(c =>
                c.location_list(true, null, null, countryId)
            );

            //var products = _client.Execute(c => c.product_list(true, null, null, 0, 100));

            //var productId = products.First().id;
            var locationId = locations.First().id;

            //// Interestingly, location ID is optional (per the docs) but not in this generated code
            //// Might be worth modifying the code to allow a null value?
            //var productStock = _client.Execute(c => c.stock_product(productId, locationId));

            var oneWeekAgo = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");

            var stockListCount = _client.Execute(c => c.stock_list_count(null, oneWeekAgo));
            var stockList = _client.Execute(c => c.stock_list(null, oneWeekAgo, 0, 100));

            var stockEntry = stockList.First(x => x.stock > 10);

            var productId = stockEntry.stkItemId;

            var productStockEntry = _client.Execute(c => c.stock_product(productId, locationId));
            var product = _client.Execute(c => c.product_get(productId));
        }
    }
}
