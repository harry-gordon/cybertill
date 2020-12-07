using System;
using System.Threading.Tasks;
using Cybertill.Soap;

namespace Cybertill
{
    public interface ICybertillClient
    {
        Task InitAsync();
        Task<T> ExecuteAsync<T>(Func<CybertillApi_v1_6PortTypeClient, Task<T>> func);
    }
}
