using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Graphapi.Utils.Models;

[ExcludeFromCodeCoverage]
public record GraphApiGroup
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;
}
