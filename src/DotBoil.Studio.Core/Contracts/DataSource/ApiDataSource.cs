namespace DotBoil.Studio.Core.Contracts;

public class ApiDataSource : DataSource
{
    public string ApiUrl { get; set; }
    public HttpMethod HttpMethod { get; set; }
    
    public override Task<DataSourceResult> GetItemsAsync(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }
}