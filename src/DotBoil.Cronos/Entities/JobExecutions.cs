namespace DotBoil.Cronos.Entities;

public class JobExecutions
{
    public Guid Id { get; set; }
    public Guid ScheduledJobId { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset FinishedAtUtc { get; set; }
    public long DurationMs { get; set; }
    public string Status { get; set; }
    public string Error { get; set; }
    
    public virtual ScheduledJobs ScheduledJob { get; set; }
}