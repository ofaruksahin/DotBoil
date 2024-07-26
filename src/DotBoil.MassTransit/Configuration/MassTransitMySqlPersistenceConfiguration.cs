using DotBoil.MassTransit.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.MassTransit.Configuration
{
    internal class MassTransitMySqlPersistenceConfiguration
    {
        public string ConnectionString { get; set; }


        public async Task<WebApplicationBuilder> ConfigurePersistence(WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<MassTransitDbContext>(options =>
            {
                options.UseMySQL(ConnectionString);
            });

            return builder;
        }
    }
}
