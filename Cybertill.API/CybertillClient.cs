using System;
using System.Text;
using System.Threading.Tasks;
using Cybertill.API.Soap;

namespace Cybertill.API
{
    public class CybertillClient : ICybertillClient
    {
        private readonly CybertillConfig _config;
        private readonly CybertillApi_v1_6Service _client;
        private CybertillApi_v1_6Service _authenticatedClient;

        public CybertillClient(CybertillConfig config)
        {
            _config = config;

            _client = new CybertillApi_v1_6Service
            {
                Url = _config.EndpointUrl
            };
        }

        public Task InitAsync()
        {
            var authResult = _client.authenticate_get(_config.Username, _config.AuthId);

            // This looks wacky but it's how Cybertill auth works
            var encodedAuthValue = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{authResult}:{authResult}"
                )
            );

            _authenticatedClient = new CybertillApi_v1_6Service
            {
                Url = "https://ct226532.c-pos.co.uk/current/CybertillApi_v1_6.php",
                AuthHeaderValue = encodedAuthValue
            };

            return Task.FromResult(0);
        }

        public T Execute<T>(Func<CybertillApi_v1_6Service, T> func)
        {
            CheckAuth();

            return func(_authenticatedClient);
        }

        private void CheckAuth()
        {
            if (_authenticatedClient == null)
            {
                throw new InvalidOperationException("Client is not authorized, please initialise the client before making an calls");
            }
        }
    }
}
