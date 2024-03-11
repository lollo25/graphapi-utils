using System.Diagnostics.CodeAnalysis;

namespace Graphapi.Utils.Models;

[ExcludeFromCodeCoverage]
public record ListDownloaderOptions : AuthenticationOptions
{
    public int Top { get; init; }
    public string Directory { get; init; } = string.Empty;
}