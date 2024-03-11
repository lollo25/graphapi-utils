using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Graphapi.Utils.Models;
[ExcludeFromCodeCoverage]
public record ClientCredentialsToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = string.Empty;
}
