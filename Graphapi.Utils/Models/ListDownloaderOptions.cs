using System.Diagnostics.CodeAnalysis;

namespace Graphapi.Utils.Models;

[ExcludeFromCodeCoverage]
public record ListDownloaderOptions : AuthenticationOptions
{
    public int PageSize { get; init; }
    public string Directory { get; init; } = string.Empty;
}