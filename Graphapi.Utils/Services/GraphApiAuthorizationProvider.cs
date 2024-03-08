using Graphapi.Utils;
using Graphapi.Utils.Models;
using LanguageExt;
using LanguageExt.Common;
using System.Text.Json;
using static LanguageExt.Prelude;

namespace Graphapi.Utils.Services;
public class GraphApiAuthorizationProvider : IAuthorizationProvider
{
    private const string ClientIdParam = "client_id";
    private const string ClientSecretParam = "client_secret";
    private readonly KeyValuePair<string, string> _clientCredentialsKvp;
    private readonly KeyValuePair<string, string> _scopeCredentialsKvp;
    private readonly Func<HttpClient> _createClient;

    public GraphApiAuthorizationProvider(IHttpClientFactory httpClientFactory)
    {
        _clientCredentialsKvp = new KeyValuePair<string, string>("grant_type", "client_credentials");
        _scopeCredentialsKvp = new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default");
        _createClient = () => httpClientFactory.CreateClient(Constants.MicrosoftLoginClient);
    }

    public EitherAsync<Error, ClientCredentialsToken> AuthenticateAsync(
        AuthenticationOptions authenticationOptions,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, string.Format(Constants.TokenUrlFormat, authenticationOptions.Tenant))
        {
            Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>(ClientIdParam, authenticationOptions.AppId),
                new KeyValuePair<string, string>(ClientSecretParam, authenticationOptions.AppSecret),
                _scopeCredentialsKvp,
                _clientCredentialsKvp
            })
        };
        return
            TryAsync(async () =>
                        (await _createClient().SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)).EnsureSuccessStatusCode())
                .Bind(res => TryAsync(async () => (await JsonSerializer.DeserializeAsync<ClientCredentialsToken>(await res.Content.ReadAsStreamAsync()))!))
                .ToEither();
    }
}
