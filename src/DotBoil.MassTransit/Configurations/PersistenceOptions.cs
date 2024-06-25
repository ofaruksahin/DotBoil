using Microsoft.EntityFrameworkCore;

namespace DotBoil.MassTransit.Configurations
{
    internal abstract class PersistenceOptions
    {
        public abstract void ConfigurePersistence(DbContextOptionsBuilder optionsBuilder);
    }
}
