using System.Text.Json.Serialization;

namespace Graphapi.Utils.Models;
public record ClientCredentialsToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
}

public record AuthenticationOptions
{
    public string Tenant { get; init; } = string.Empty;
    public string AppId { get; init; } = string.Empty;
    public string AppSecret { get; init; } = string.Empty;
}