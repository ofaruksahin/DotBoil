using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.JSInterop;

namespace DotBoil.Studio.Core.Services;

public class JwtAuthService
{
    private readonly IJSRuntime _js;
    private string _token;

    public JwtAuthService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SetTokenAsync(string token)
    {
        _token = token;
        await _js.InvokeVoidAsync("localStorage.setItem", "jwt_token", token);
    }

    public async Task<string> GetTokenAsync()
    {
        if (!string.IsNullOrEmpty(_token))
            return _token;

        _token = await _js.InvokeAsync<string>("localStorage.getItem", "jwt_token");
        return _token;
    }

    public async Task<bool> IsTokenValidAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            var payload = token.Split('.')[1];
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
        _token = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", "jwt_token");
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