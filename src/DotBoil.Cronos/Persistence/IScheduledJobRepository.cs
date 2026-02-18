using DotBoil.Cronos.Entities;

namespace DotBoil.Cronos.Persistence;

public interface IScheduledJobRepository
{
    Task<IReadOnlyList<ScheduledJobs>> GetAllAsync();
    Task InvalidateCacheAsync();
}
