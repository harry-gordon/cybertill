using Cybertill.Soap;
using System;
using System.ServiceModel.Dispatcher;
using System.Threading;
using System.Threading.Tasks;

namespace Cybertill.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            const string endpointUrl = "https://ct226532.c-pos.co.uk/current/CybertillApi_v1_6.php";
            var timeout = TimeSpan.FromSeconds(120);

            var authClient =
                new CybertillApi_v1_6PortTypeClient(
                    endpointUrl,
                    timeout
                );

            const string website = "www.untouchables.co.uk";
            const string authId = "28e6376f9917ad10ab7ea986212998c1";

            var authResult = await authClient.authenticate_getAsync(website, authId);

            var client =
                new CybertillApi_v1_6PortTypeClient(
                    endpointUrl,
                    timeout,
                    authResult,
                    string.Empty
                );

            var categories = await client.category_listAsync();
        }
    }
}
