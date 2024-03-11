using System.Diagnostics.CodeAnalysis;

namespace Graphapi.Utils.Models;

[ExcludeFromCodeCoverage]
public record AuthenticationOptions
{
    public string Tenant { get; init; } = string.Empty;
    public string AppId { get; init; } = string.Empty;
    public string AppSecret { get; init; } = string.Empty;
}
