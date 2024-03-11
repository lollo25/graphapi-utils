using System.Diagnostics.CodeAnalysis;

namespace Graphapi.Utils.Models;

[ExcludeFromCodeCoverage]
public record ListDownloaderOptions : AuthenticationOptions
{
    public string LoginApiRootUrl { get; init; } = string.Empty;
    public string GraphApiRootUrl { get; init; } = string.Empty;
    public string GraphApiVersion { get; init; } = string.Empty;
    public int PageSize { get; init; }
    public string Directory { get; init; } = string.Empty;
}