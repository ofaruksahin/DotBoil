namespace DotBoil.Studio.Core.Contracts;

public class DataSourceResult
{
    public IEnumerable<object> Items { get; set; }
    public int? TotalCount { get; set; }
}