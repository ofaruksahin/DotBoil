using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DotBoil.Studio.Core.Services;
using DotBoil.Studio.Core.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Studio.Core.Contracts;

public class ApiDataSource : DataSource
{
    public string BaseUrl { get; set; }
    public string ApiUrl { get; set; }
    public HttpMethod HttpMethod { get; set; }
    
    [JsonIgnore]
    private JwtAuthService _jwtAuthService { get; set; }

    [JsonIgnore]
    private string NormalizedUrl { get; set; }
    
    public bool OverrideApiUrl(ComponentContext context, JwtAuthService jwtAuthService)
    {
        _jwtAuthService = jwtAuthService;

        if (string.IsNullOrEmpty(BaseUrl))
            return false;

        if (string.IsNullOrEmpty(ApiUrl))
            return false;

        var normalizedUrl = string.Format("{0}/{1}",
            BaseUrl.Trim('/'),
            ApiUrl.Trim('/'));
        
        NormalizedUrl = normalizedUrl;

        if (!normalizedUrl.Contains("{") || !normalizedUrl.Contains("}"))
            return true;
        
        var matches = Regex.Matches(normalizedUrl, "{(.*?)}");
        
        var parameters = matches
            .Select(m => m.Groups[1].Value)
            .ToList();

        foreach (var parameter in parameters)
        {
            var component = context.Components.FirstOrDefault(c => c.Id == parameter);

            if (component is null)
                return false;

            var property = component.GetType()
                .GetProperties()
                .FirstOrDefault(p => p.GetCustomAttributes(true)
                    .Any(a => a.GetType().Name == "FieldOutputPropertyAttribute"));

            if (property == null)
                return false;

            var value = property.GetValue(component);

            string valueString;

            if (value is System.Collections.IEnumerable enumerable && value is not string)
            {
                var first = enumerable.Cast<object>().FirstOrDefault();
                valueString = first?.ToString() ?? string.Empty;
            }
            else
            {
                valueString = value?.ToString() ?? string.Empty;
            }

            if (string.IsNullOrEmpty(valueString))
                return false;

            normalizedUrl = normalizedUrl.Replace($"{{{parameter}}}", valueString);
        }
        
        NormalizedUrl = normalizedUrl;

        return true;
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
        
        var client = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
        
        if (!BaseUrl.EndsWith('/'))
            BaseUrl += '/';
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtAuthService.Token);
        
        var request = new HttpRequestMessage(HttpMethod, NormalizedUrl.Trim('/'));
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