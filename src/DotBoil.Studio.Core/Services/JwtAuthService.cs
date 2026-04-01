using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DotBoil.Configuration;
using DotBoil.Studio.Core.Configurations;
using DotBoil.Studio.Core.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;

namespace DotBoil.Studio.Core.Services;

public class JwtAuthService
{
    private readonly IJSRuntime _js;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    
    public string Token;
    public string RefreshToken;

    public JwtAuthService(
        IJSRuntime js,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _js = js;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        Token = await _js.InvokeAsync<string>("localStorage.getItem", "access_token");
        RefreshToken = await _js.InvokeAsync<string>("localStorage.getItem", "refresh_token");
    }

    public async Task SetTokenAsync(string token)
    {
        Token = token;
        await _js.InvokeVoidAsync("localStorage.setItem", "access_token", token);
    }

    public async Task SetRefreshTokenAsync(string refreshToken)
    {
        RefreshToken = refreshToken;
        await _js.InvokeVoidAsync("localStorage.setItem", "refresh_token", refreshToken);
    }

    public async Task<bool> GetNewTokenWithRefreshTokenAsync()
    {
        var authServerConfiguration = _configuration.GetConfigurations<AuthServerConfiguration>();
        
        var url = authServerConfiguration.Url.Trim('/');
        var refreshTokenEndpoint = authServerConfiguration.RefreshTokenEndpoint.Trim('/');
        var baseUrl = new Uri(string.Format("{0}/{1}?refreshToken={2}", url, refreshTokenEndpoint, RefreshToken));
        
        var request = new HttpRequestMessage(HttpMethod.Post, baseUrl);
        var response = await this._httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return false;

        var refreshTokenResult = await response.Content.ReadFromJsonAsync<RefreshTokenResult>();

        if (refreshTokenResult == null || !refreshTokenResult.IsSuccess)
            return false;
        
        Token = refreshTokenResult.AccessToken;
        
        return true;
    }

    public bool IsTokenValidAsync()
    {
        if (string.IsNullOrEmpty(Token))
            return false;

        try
        {
            var payload = Token.Split('.')[1];
            var json = Base64UrlDecode(payload);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("exp", out var expElement))
            {
                var expUnix = expElement.GetInt64();
                var expTime = DateTimeOffset.FromUnixTimeSeconds(expUnix);

                return expTime > DateTimeOffset.UtcNow;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    public async Task ClearTokenAsync()
    {
        Token = null;
        RefreshToken = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", "access_token");
        await _js.InvokeVoidAsync("localStorage.removeItem", "refresh_token");
    }

    public async Task<IEnumerable<MenuItem>> GetMenuItems()
    {
        var authServerConfiguration = _configuration.GetConfigurations<AuthServerConfiguration>();
        
        var url = authServerConfiguration.Url.Trim('/');
        var menuEndpoint = authServerConfiguration.MenuEndpoint.Trim('/');
        var baseUrl = new Uri(string.Format("{0}/{1}", url, menuEndpoint));
        
        var request = new HttpRequestMessage(HttpMethod.Post, baseUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        var response = await this._httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return Array.Empty<MenuItem>();
        
        var menuResult = await response.Content.ReadFromJsonAsync<List<MenuItem>>();
        
        return menuResult ?? new List<MenuItem>();
    }
    
    private string Base64UrlDecode(string input)
    {
        input = input.Replace('-', '+').Replace('_', '/');
        switch (input.Length % 4)
        {
            case 2: input += "=="; break;
            case 3: input += "="; break;
        }

        var bytes = Convert.FromBase64String(input);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}