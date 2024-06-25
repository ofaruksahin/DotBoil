using Microsoft.EntityFrameworkCore;

namespace DotBoil.MassTransit.Configurations
{
    internal class MySqlOptions : PersistenceOptions
    {
        public string ConnectionString { get; set; }

        public override void ConfigurePersistence(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(ConnectionString);
        }
    }
}
