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

            var client =
                new CybertillApi_v1_6PortTypeClient(
                    cybertillConfig.EndpointUrl,
                    timeout
                );
            
            var authResult = await client.authenticate_getAsync(cybertillConfig.Username, cybertillConfig.AuthId);

            Task<country_listResponse> countryListTask;
            using (var scope = new OperationContextScope(client.InnerChannel))
            {
                // Add SOAP Header to an outgoing request
                var header = MessageHeader.CreateHeader("Authentication", string.Empty, $"Basic {authResult}");
                OperationContext.Current.OutgoingMessageHeaders.Add(header);

                /*
                // Add HTTP Header to an outgoing request
                var requestMessage = new HttpRequestMessageProperty();
                requestMessage.Headers["Authentication"] = $"Basic {authResult}";
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name]
                    = requestMessage;
                */

                countryListTask = client.country_listAsync();
            }

            var countryList = await countryListTask;
        }
    }
}
