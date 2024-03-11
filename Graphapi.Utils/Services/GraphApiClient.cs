using Graphapi.Utils.Models;
using LanguageExt;
using LanguageExt.Common;
using System.Net.Http.Headers;
using System.Text.Json;
using static LanguageExt.Prelude;
using static System.Net.HttpStatusCode;

namespace Graphapi.Utils.Services;
public class GraphApiClient<T> : IGraphApiClient<T>
{
    private readonly Func<string, HttpClient> _createClient;

    public GraphApiClient(IHttpClientFactory httpClientFactory)
    {
        _createClient = (string accessToken) =>
        {
            var client = httpClientFactory.CreateClient(Constants.GraphApiClient);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    Constants.LoginAuthScheme,
                    accessToken);
            return client;
        };
    }

    public EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<T>>> GetPagedAsync(
        string address,
        string accessToken,
        CancellationToken cancellationToken) =>
            TryAsync(async () =>
                    (await _createClient(accessToken)
                        .GetAsync(
                            address,
                            cancellationToken))!)
                .ToEither()
                .Bind(MapHttpResponse);

    private EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<T>>> MapHttpResponse(HttpResponseMessage response) =>
        response.StatusCode switch
        {
            TooManyRequests =>
                (EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<T>>>)
                    (Either<ThrottledResponse, GraphApiPagedResponse<T>>)CreateThrottledResponse(response),
            OK =>
                TryAsync(async () =>
                        await JsonSerializer.DeserializeAsync<GraphApiPagedResponse<T>>(await response.Content.ReadAsStreamAsync()))
                .ToEither()
                .BiMap(
                    _ => (Either<ThrottledResponse, GraphApiPagedResponse<T>>)_!,
                    err => Error.New((int)response.StatusCode, "The request was successful but the serialization of the error model fails.")),
            _ =>
                TryAsync(async () =>
                        (await JsonSerializer.DeserializeAsync<GraphApiError>(await response.Content.ReadAsStreamAsync())))
                .ToEither()
                .Match(
                    _ => Error.New((int)response.StatusCode, $"The request was not successful with message: {_!.Error.Message}"),
                    err => Error.New((int)response.StatusCode, "The request was not successful and the serialization of the error model fails."))
        };

    private static ThrottledResponse CreateThrottledResponse(HttpResponseMessage response) =>
        response
            .Headers
            .Find(kvp => kvp.Key == Constants.TooManyRequestRetryAfterHeaderKey)
            .Map(kvp => new ThrottledResponse { RetryAfter = Convert.ToInt32(kvp.Value.First()) })
            .IfNone(new ThrottledResponse());
}
