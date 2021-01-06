using Cybertill.Services;

namespace Cybertill.Console
{
    public class CybertillExactNameLocationSelector : ICybertillLocationSelector
    {
        private readonly string _name;

        public CybertillExactNameLocationSelector(string name)
        {
            _name = name;
        }

        public bool IsLocation(string locationName, string locationArea)
        {
            return locationName == _name;
        }
    }
}
