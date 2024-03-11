using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Graphapi.Utils.Models;

[ExcludeFromCodeCoverage]
public record GraphApiGroup
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("deletedDateTime")]
    public DateTime? DeletedDateTime { get; init; }

    [JsonPropertyName("classification")]
    public string? Classification { get; init; }

    [JsonPropertyName("createdDateTime")]
    public DateTime CreatedDateTime { get; init; }

    [JsonPropertyName("creationOptions")]
    public string[]? CreationOptions { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("expirationDateTime")]
    public DateTime? ExpirationDateTime { get; init; }

    [JsonPropertyName("groupTypes")]
    public string[]? GroupTypes { get; init; }

    [JsonPropertyName("isAssignableToRole")]
    public bool? IsAssignableToRole { get; init; }

    [JsonPropertyName("mail")]
    public string? Mail { get; init; }

    [JsonPropertyName("mailEnabled")]
    public bool MailEnabled { get; init; }

    [JsonPropertyName("mailNickname")]
    public string? MailNickname { get; init; }

    [JsonPropertyName("membershipRule")]
    public string? MembershipRule { get; init; }

    [JsonPropertyName("membershipRuleProcessingState")]
    public string? MembershipRuleProcessingState { get; init; }

    [JsonPropertyName("onPremisesDomainName")]
    public string? OnPremisesDomainName { get; init; }

    [JsonPropertyName("onPremisesLastSyncDateTime")]
    public DateTimeOffset? OnPremisesLastSyncDateTime { get; init; }

    [JsonPropertyName("onPremisesNetBiosName")]
    public string? OnPremisesNetBiosName { get; init; }

    [JsonPropertyName("onPremisesSamAccountName")]
    public string? OnPremisesSamAccountName { get; init; }

    [JsonPropertyName("onPremisesSecurityIdentifier")]
    public string? OnPremisesSecurityIdentifier { get; init; }

    [JsonPropertyName("onPremisesSyncEnabled")]
    public string? OnPremisesSyncEnabled { get; init; }

    [JsonPropertyName("preferredDataLocation")]
    public string? PreferredDataLocation { get; init; }

    [JsonPropertyName("preferredLanguage")]
    public string? PreferredLanguage { get; init; }

    [JsonPropertyName("proxyAddresses")]
    public string[]? ProxyAddresses { get; init; }

    [JsonPropertyName("renewedDateTime")]
    public DateTime RenewedDateTime { get; init; }

    [JsonPropertyName("resourceBehaviorOptions")]
    public string[]? ResourceBehaviorOptions { get; init; }

    [JsonPropertyName("resourceProvisioningOptions")]
    public string[]? ResourceProvisioningOptions { get; init; }

    [JsonPropertyName("securityEnabled")]
    public bool SecurityEnabled { get; init; }

    [JsonPropertyName("securityIdentifier")]
    public string SecurityIdentifier { get; init; } = string.Empty;

    [JsonPropertyName("theme")]
    public string? Theme { get; init; }

    [JsonPropertyName("visibility")]
    public string? Visibility { get; init; }

    [JsonPropertyName("onPremisesProvisioningErrors")]
    public OnPremisesProvisioningError[]? OnPremisesProvisioningErrors { get; init; }

    [JsonPropertyName("serviceProvisioningErrors")]
    public ServiceProvisioningError[]? ServiceProvisioningErrors { get; init; }
}
[ExcludeFromCodeCoverage]
public record OnPremisesProvisioningError
{
    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("occurredDateTime")]
    public DateTimeOffset OccurredDateTime { get; init; }

    [JsonPropertyName("propertyCausingError")]
    public string PropertyCausingError { get; init; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;
}
[ExcludeFromCodeCoverage]
public record ServiceProvisioningError
{
    [JsonPropertyName("createdDateTime")]
    public DateTime CreatedDateTime { get; init; }

    [JsonPropertyName("isResolved")]
    public bool IsResolved { get; init; }

    [JsonPropertyName("serviceInstance")]
    public string ServiceInstance { get; init; } = string.Empty;
}