using DotBoil.Caching.Configuration.Redis;
using DotBoil.Caching.Redis;
using DotBoil.Configuration;
using DotBoil.Dependency;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DotBoil.Caching
{
    internal class CachingModule : Module
    {
        public override Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            var options = builder.Configuration.GetConfigurations<RedisConfiguration>();

            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionMultiplexerOptions = new ConfigurationOptions();

                options.Endpoints.ForEach(endpoint => connectionMultiplexerOptions.EndPoints.Add(endpoint.IpAddress, endpoint.Port));

                connectionMultiplexerOptions.Password = options.Password;

                return ConnectionMultiplexer.Connect(connectionMultiplexerOptions);
            });

            builder.Services.AddSingleton<ICache, RedisCache>();

            return Task.FromResult(builder);
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            return Task.FromResult(app);
        }
    }
}
