using DotBoil.Configuration;
using DotBoil.Cronos.Configuration;
using DotBoil.Cronos.Entities;
using DotBoil.Serialization;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace DotBoil.Cronos.Persistence;

internal class ScheduledJobRepository : IScheduledJobRepository
{
    private const string JobsCacheKey = "DotBoil:Cronos:ScheduledJobs";

    private readonly CronosDbContext _dbContext;
    private readonly IDatabase _database;

    public ScheduledJobRepository(CronosDbContext dbContext)
    {
        _dbContext = dbContext;

        var configuration = DotBoilApp.Configuration.GetConfigurations<CronosConfiguration>();
        var connectionMultiplexer = ConnectionMultiplexer.Connect(configuration.RedisConnectionString);
        _database = connectionMultiplexer.GetDatabase(0);
    }

    public async Task<IReadOnlyList<ScheduledJobs>> GetAllAsync()
    {
        var cached = await _database.StringGetAsync(JobsCacheKey);
        if (cached.HasValue)
            return await cached.ToString().DeserializeAsync<List<ScheduledJobs>>();

        var jobs = await _dbContext
            .ScheduledJobs
            .Include(j => j.JobExecutions)
            .ToListAsync();

        var json = await jobs.SerializeAsync();
        await _database.StringSetAsync(JobsCacheKey, json);

        return jobs;
    }

    public async Task InvalidateCacheAsync()
    {
        await _database.KeyDeleteAsync(JobsCacheKey, CommandFlags.FireAndForget);
    }
}
