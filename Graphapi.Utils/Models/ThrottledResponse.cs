using System.Diagnostics.CodeAnalysis;

namespace Graphapi.Utils.Models;

[ExcludeFromCodeCoverage]
public record ThrottledResponse
{
    public int RetryAfter { get; init; }
}
