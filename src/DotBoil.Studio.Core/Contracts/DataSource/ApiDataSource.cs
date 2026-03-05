using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DotBoil.Entities;
using DotBoil.Studio.Core.Services;

namespace DotBoil.Studio.Core.Contracts;

public class ApiDataSource : DataSource
{
    public string BaseUrl { get; set; }
    public string ApiUrl { get; set; }
    public HttpMethod HttpMethod { get; set; }
    
    [JsonIgnore]
    private readonly IHttpClientFactory _httpClientFactory;
    
    private JwtAuthService _jwtAuthService { get; set; }

    public ApiDataSource(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public void OverrideApiUrl(ComponentContext context, JwtAuthService jwtAuthService)
    {
        _jwtAuthService = jwtAuthService;
    }
    
    public override async Task<DataSourceResult> GetItemsAsync(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(BaseUrl))
            return new DataSourceResult();

        if (string.IsNullOrEmpty(ApiUrl))
            return new DataSourceResult();
        
        if (HttpMethod == null)
            return new DataSourceResult();
        
        if (_jwtAuthService == null)
            throw new ArgumentNullException(nameof(JwtAuthService));
        
        var client = _httpClientFactory.CreateClient();
        
        if (!BaseUrl.EndsWith('/'))
            BaseUrl += '/';
        
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtAuthService.Token);
        
        var request = new HttpRequestMessage(HttpMethod, ApiUrl.TrimStart('/'));
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                await _jwtAuthService.GetNewTokenWithRefreshTokenAsync();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtAuthService.Token);
                response = await client.SendAsync(request);
            }
        }
        
        if (!response.IsSuccessStatusCode)
            return new DataSourceResult();

        var responseBody = await response.Content.ReadFromJsonAsync<ApiDataSourceBaseResponse>();

        if (responseBody == null || !responseBody.Data.Any())
            return new DataSourceResult();

        return new DataSourceResult()
        {
            Items = responseBody.Data
        };
    }
}