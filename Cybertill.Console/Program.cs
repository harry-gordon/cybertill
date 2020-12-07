using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Cybertill.Console
{
    internal class Program
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
                .AddUserSecrets(Assembly.GetAssembly(typeof(Program)))
                .AddEnvironmentVariables()
                .Build();

            var config = new CybertillConfig();
            configuration.Bind("Cybertill", config);

            _client = new CybertillClient(config);

            await _client.InitAsync();

            var categories = await _client.ExecuteAsync(c => c.category_listAsync());

            //await StockCheckAsync();
        }

        static async Task StockCheckAsync()
        {
            //var categories = await _client.ExecuteAsync(c => c.category_listAsync());
            var countries = await _client.ExecuteAsync(c => c.country_listAsync());

            var countryId = countries.result.First().id;

            var locations = await _client.ExecuteAsync(c => c.location_listAsync(true, string.Empty, string.Empty, countryId));

            var dummyProductId = 5;
            var locationId = locations.result.First().id;

            // Interestingly, location ID is optional but not in this generated code
            // Might be worth modifying the code to allow a null value?
            var productStock = await _client.ExecuteAsync(c => c.stock_productAsync(dummyProductId, locationId));

            // It's unclear how this query works
            //var stock = await client.ExecuteAsync(c => c.stock_listAsync("2018-01-01", "2018-01-10", 0, 100));
        }
    }
}
