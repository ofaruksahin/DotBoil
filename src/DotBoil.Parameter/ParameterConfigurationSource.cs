using Microsoft.Extensions.Configuration;

namespace DotBoil.Parameter
{
    internal class ParameterConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ParameterConfigurationProvider();
        }
    }
}
