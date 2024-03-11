using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Graphapi.Utils.Models;

[ExcludeFromCodeCoverage]
public record GraphApiPagedResponse<T>
{
    [JsonPropertyName("@odata.nextLink")]
    public Uri? Next { get; init; }
    [JsonPropertyName("value")]
    public T[] Value { get; init; } = Array.Empty<T>();
}
