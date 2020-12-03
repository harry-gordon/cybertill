using Cybertill.Soap;
using System;
using System.ServiceModel.Dispatcher;

namespace Cybertill.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var client =
                new CybertillApi_v1_6PortTypeClient(
                    "https://www.untouchables.co.uk",
                    TimeSpan.FromSeconds(120),
                    "",
                    "28e6376f9917ad10ab7ea986212998c1"
                );

            var categories = client.country_listAsync();
        }
    }
}
