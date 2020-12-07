using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Cybertill.Console;
using Cybertill.Soap;

namespace Cybertill
{
    public class CybertillClient : ICybertillClient
    {
        private readonly CybertillConfig _config;
        private readonly CybertillApi_v1_6PortTypeClient _client;
        private string _authHeaderValue;

        public CybertillClient(CybertillConfig config)
        {
            _config = config;

            var timeout = TimeSpan.FromSeconds(120);

            _client =
                new CybertillApi_v1_6PortTypeClient(
                    _config.EndpointUrl,
                    timeout
                );
        }

        public async Task InitAsync()
        {
            var authResult = await _client.authenticate_getAsync(_config.Username, _config.AuthId);
            // This looks wacky but it's how Cybertill auth works
            authResult = $"{authResult}:{authResult}";
            _authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(authResult));
        }

        public Task<T> ExecuteAsync<T>(Func<CybertillApi_v1_6PortTypeClient, Task<T>> func)
        {
            CheckAuth();

            using (var scope = new OperationContextScope(_client.InnerChannel))
            {
                // Add HTTP auth header
                var requestMessage = new HttpRequestMessageProperty();
                requestMessage.Headers["Authorization"] = $"Basic {_authHeaderValue}";
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name]
                    = requestMessage;

                return func(_client);
            }
        }

        private void CheckAuth()
        {
            if (_authHeaderValue == null)
            {
                throw new InvalidOperationException("Client is not authorized, please initialise the client before making an calls");
            }
        }
    }
}
