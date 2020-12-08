using System;
using System.Threading.Tasks;
using Cybertill.API.Soap;

namespace Cybertill.API
{
    public interface ICybertillClient
    {
        Task InitAsync();
        T Execute<T>(Func<CybertillApi_v1_6Service, T> func);
    }
}
