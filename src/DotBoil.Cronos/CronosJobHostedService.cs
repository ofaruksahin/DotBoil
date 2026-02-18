using System.Diagnostics;
using Cronos;
using DotBoil.Cronos.Entities;
using DotBoil.Cronos.Persistence;
using DotBoil.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotBoil.Cronos;

internal class CronosJobHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CronosJobHostedService> _logger;

    public CronosJobHostedService(
        IServiceProvider serviceProvider,
        ILogger<CronosJobHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ExecuteJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CronosJobHostedService execution loop failed.");
            }
        }
    }

    private async Task ExecuteJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<Persistence.CronosDbContext>();
        var jobRepository = scope.ServiceProvider.GetRequiredService<IScheduledJobRepository>();

        var nowUtc = DateTimeOffset.UtcNow;
        var jobs = await jobRepository.GetAllAsync();

        var dueJobs = jobs
            .Where(j => j.IsActive && j.NextRunAtUtc <= nowUtc)
            .ToList();

        if (!dueJobs.Any())
            return;

        foreach (var job in dueJobs)
        {
            if (string.IsNullOrWhiteSpace(job.TypeName))
                continue;

            var jobType = AppDomain.CurrentDomain.FindType(job.TypeName);
            if (jobType is null)
                continue;

            var cronJob = (ICronosJob)scope.ServiceProvider.GetRequiredService(jobType);

            var execution = new JobExecutions
            {
                Id = Guid.NewGuid(),
                ScheduledJobId = job.Id,
                StartedAtUtc = nowUtc
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await cronJob.ExecuteAsync(cancellationToken);

                execution.Status = "Success";
            }
            catch (Exception ex)
            {
                execution.Status = "Failed";
                execution.Error = ex.ToString();

                _logger.LogError(ex,
                    "Cron job '{JobName}' failed during execution.",
                    job.Name);
            }

            stopwatch.Stop();

            execution.FinishedAtUtc = DateTimeOffset.UtcNow;
            execution.DurationMs = stopwatch.ElapsedMilliseconds;

            dbContext.JobExecutions.Add(execution);

            var jobEntity = await dbContext.ScheduledJobs.FirstOrDefaultAsync(j => j.Id == job.Id, cancellationToken);
            if (jobEntity is not null)
            {
                jobEntity.LastRunAtUtc = execution.FinishedAtUtc;
                jobEntity.LastDurationMs = execution.DurationMs;
                jobEntity.LastStatus = execution.Status;
                jobEntity.LastError = execution.Error;

                if (!string.IsNullOrWhiteSpace(jobEntity.CronExpression))
                {
                    try
                    {
                        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                            string.IsNullOrWhiteSpace(jobEntity.TimeZoneId)
                                ? TimeZoneInfo.Utc.Id
                                : jobEntity.TimeZoneId);

                        var cronExpression = CronExpression.Parse(jobEntity.CronExpression);
                        var next = cronExpression.GetNextOccurrence(DateTimeOffset.UtcNow, timeZone);
                        if (next.HasValue)
                        {
                            jobEntity.NextRunAtUtc = next.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to calculate next run time for job '{JobName}' with expression '{Expression}'.",
                            jobEntity.Name,
                            jobEntity.CronExpression);
                    }
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
