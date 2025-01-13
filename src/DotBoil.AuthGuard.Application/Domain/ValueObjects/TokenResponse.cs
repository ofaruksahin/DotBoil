using System.Text.Json.Serialization;

namespace DotBoil.AuthGuard.Application.Domain.ValueObjects;

internal class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}