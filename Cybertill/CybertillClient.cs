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
        private string _authValue;

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
            authResult = $"{authResult}:{authResult}";
            _authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(authResult));
        }

        public Task<T> ExecuteAsync<T>(Func<CybertillApi_v1_6PortTypeClient, Task<T>> func)
        {
            CheckAuth();

            using (var scope = new OperationContextScope(_client.InnerChannel))
            {
                //// Add SOAP Header to an outgoing request
                //var header = MessageHeader.CreateHeader("Authentication", string.Empty, $"Basic {_authValue}");
                //OperationContext.Current.OutgoingMessageHeaders.Add(header);

                // Add HTTP Header to an outgoing request
                var requestMessage = new HttpRequestMessageProperty();
                requestMessage.Headers["Authorization"] = $"Basic {_authValue}";
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name]
                    = requestMessage;

                return func(_client);
            }
        }

        private void CheckAuth()
        {
            if (_authValue == null)
            {
                throw new InvalidOperationException("Client is not authorized, please initialise the client before making an calls");
            }
        }
    }
}
