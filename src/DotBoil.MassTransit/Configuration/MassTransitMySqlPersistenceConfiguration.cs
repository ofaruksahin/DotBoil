using DotBoil.MassTransit.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.MassTransit.Configuration
{
    internal class MassTransitMySqlPersistenceConfiguration
    {
        public string ConnectionString { get; set; }


        public Task ConfigurePersistence()
        {
            DotBoilApp.Services.AddDbContext<MassTransitDbContext>(options =>
            {
                options.UseMySQL(ConnectionString);
            });

            return Task.CompletedTask;
        }
    }
}
