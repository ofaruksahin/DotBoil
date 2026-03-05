namespace DotBoil.Studio.Core.Contracts;

public class StaticDataSource : DataSource
{
    public string Text { get; set; }
    public string Value { get; set; }
    public bool IsSelected { get; set; }
    
    public List<object> Items { get; set; } = new();
    
    public override Task<DataSourceResult> GetItemsAsync(IServiceProvider serviceProvider)
    {
        return Task.FromResult(new DataSourceResult()
        {
            Items = Items,
            TotalCount = Items.Count
        });
    }
}