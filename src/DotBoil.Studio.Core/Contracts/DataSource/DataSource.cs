namespace DotBoil.Studio.Core.Contracts;

public abstract class DataSource
{
    public abstract Task<DataSourceResult> GetItemsAsync(IServiceProvider serviceProvider);
}