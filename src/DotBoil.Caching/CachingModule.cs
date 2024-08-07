using DotBoil.Caching.Configuration.Redis;
using DotBoil.Caching.Redis;
using DotBoil.Configuration;
using DotBoil.Dependency;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace DotBoil.Caching
{
    internal class CachingModule : Module
    {
        public override Task AddModule()
        {
            var options = DotBoilApp.Configuration.GetConfigurations<RedisConfiguration>();

            DotBoilApp.Services.TryAddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionMultiplexerOptions = new ConfigurationOptions();

                options.Endpoints.ForEach(endpoint => connectionMultiplexerOptions.EndPoints.Add(endpoint.IpAddress, endpoint.Port));

                connectionMultiplexerOptions.Password = options.Password;

                return ConnectionMultiplexer.Connect(connectionMultiplexerOptions);
            });

            DotBoilApp.Services.TryAddSingleton<ICache, RedisCache>();

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            return Task.CompletedTask;
        }
    }
}
