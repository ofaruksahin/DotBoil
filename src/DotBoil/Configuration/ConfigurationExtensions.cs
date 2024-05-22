using Microsoft.Extensions.Configuration;

namespace DotBoil.Configuration
{
    public static class ConfigurationExtensions
    {
        public static T GetConfigurations<T>(this IConfiguration configuration)
            where T : IOptions
        {
            var options = (T)Activator.CreateInstance(typeof(T));

            configuration.GetSection(options.Key).Bind(options);

            return options;
        }
    }
}
