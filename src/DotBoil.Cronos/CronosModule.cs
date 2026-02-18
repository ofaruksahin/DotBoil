using DotBoil.Configuration;
using DotBoil.Cronos.Configuration;
using DotBoil.Cronos.Persistence;
using DotBoil.Dependency;
using DotBoil.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Cronos;

public class CronosModule : Module
{
    public override string Name => "Cronos";
    public override IEnumerable<string> DependsOn { get; } = Enumerable.Empty<string>();
    public override int Order { get; } = 0;
    public override Task AddModule()
    {
        DotBoilApp.Services.AddDbContext<CronosDbContext>();
        DotBoilApp.Services.AddScoped<IScheduledJobRepository, ScheduledJobRepository>();

        var configuration = DotBoilApp.Configuration.GetConfigurations<CronosConfiguration>();

        if (configuration.Jobs is not null)
        {
            foreach (var job in configuration.Jobs)
            {
                if (string.IsNullOrWhiteSpace(job.TypeName))
                    continue;

                var jobType = AppDomain.CurrentDomain.FindType(job.TypeName);
                if (jobType is null)
                    continue;

                DotBoilApp.Services.AddScoped(jobType);
            }
        }

        DotBoilApp.Services.AddHostedService<CronosJobHostedService>();

        return Task.CompletedTask;
    }

    public override async Task UseModule()
    {
        using var scope = DotBoilApp.Host.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<CronosDbContext>();
        var configuration = DotBoilApp.Configuration.GetConfigurations<CronosConfiguration>();

        try
        {
            await context.Database.MigrateAsync();

            if (configuration.Jobs is not null)
            {
                foreach (var job in configuration.Jobs)
                {
                    if (string.IsNullOrWhiteSpace(job.Name) || string.IsNullOrWhiteSpace(job.TypeName))
                        continue;

                    var existing = await context.ScheduledJobs.FirstOrDefaultAsync(j => j.Name == job.Name);

                    if (existing is null)
                    {
                        var newJob = new Entities.ScheduledJobs
                        {
                            Id = Guid.NewGuid(),
                            Name = job.Name,
                            TypeName = job.TypeName,
                            CronExpression = job.CronExpression,
                            TimeZoneId = string.IsNullOrWhiteSpace(job.TimeZoneId)
                                ? TimeZoneInfo.Utc.Id
                                : job.TimeZoneId,
                            IsActive = job.IsActive,
                            LastRunAtUtc = DateTimeOffset.MinValue,
                            NextRunAtUtc = DateTimeOffset.UtcNow,
                            LastDurationMs = 0,
                            LastStatus = string.Empty,
                            LastError = string.Empty,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        context.ScheduledJobs.Add(newJob);
                    }
                    else
                    {
                        existing.TypeName = job.TypeName;
                        existing.CronExpression = job.CronExpression;
                        existing.TimeZoneId = string.IsNullOrWhiteSpace(job.TimeZoneId)
                            ? TimeZoneInfo.Utc.Id
                            : job.TimeZoneId;
                        existing.IsActive = job.IsActive;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await context.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
