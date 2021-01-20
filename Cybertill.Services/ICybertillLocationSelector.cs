using Cybertill.API.Soap;

namespace Cybertill.Services
{
    public interface ICybertillLocationSelector
    {
        bool IsLocation(string locationName, string locationArea);
    }
}
