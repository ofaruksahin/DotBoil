namespace DotBoil.Cronos.Entities;

public class ScheduledJobs
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string TypeName { get; set; }
    public string CronExpression { get; set; }
    public string TimeZoneId { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset LastRunAtUtc { get; set; }
    public DateTimeOffset NextRunAtUtc { get; set; }
    public long LastDurationMs { get; set; }
    public string LastStatus { get; set; }
    public string LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public virtual ICollection<JobExecutions> JobExecutions { get; set; }
}
