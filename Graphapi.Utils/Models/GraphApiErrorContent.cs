using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Graphapi.Utils.Models;

[ExcludeFromCodeCoverage]
public record GraphApiErrorContent
{
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}