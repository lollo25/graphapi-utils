using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Graphapi.Utils.Models;

[ExcludeFromCodeCoverage]
public record GraphApiError
{
    [JsonPropertyName("error")]
    public GraphApiErrorContent Error { get; init; } = new();
}
