using Cybertill.Soap;
using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets(Assembly.GetAssembly(typeof(Program)))
                .AddEnvironmentVariables()
                .Build();

            var cybertillConfig = new CybertillConfig();
            configuration.Bind("Cybertill", cybertillConfig);

            var timeout = TimeSpan.FromSeconds(120);

            var authClient =
                new CybertillApi_v1_6PortTypeClient(
                    cybertillConfig.EndpointUrl,
                    timeout
                );
            
            var authResult = await authClient.authenticate_getAsync(cybertillConfig.Username, cybertillConfig.AuthId);

            // This looks wrong (and doesn't work) but is based on the PHP sample
            var client =
                new CybertillApi_v1_6PortTypeClient(
                    cybertillConfig.EndpointUrl,
                    timeout
                );

            var scope = new OperationContextScope(client.InnerChannel);

            // Add a SOAP Header to an outgoing request
            var header = MessageHeader.CreateHeader("Authentication", string.Empty, $"Basic {authResult}");
            OperationContext.Current.OutgoingMessageHeaders.Add(header);

            var categories = await client.country_listAsync();
        }
    }
}
