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
                    "Bearer",
                    accessToken);
            return client;
        };
    }

    public EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<T>>> GetAsync(
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
                .Map(_ => (Either<ThrottledResponse, GraphApiPagedResponse<T>>)_!),
            _ =>
                TryAsync(async () =>
                        (await JsonSerializer.DeserializeAsync<GraphApiError>(await response.Content.ReadAsStreamAsync())))
                .ToEither()
                .Bind(_ =>
                    (EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<T>>>)Error.New(_!.Error.Message))
        };

    private static ThrottledResponse CreateThrottledResponse(HttpResponseMessage response) =>
        response
            .Headers
            .Find(kvp => kvp.Key == "Retry-After")
            .Map(kvp => new ThrottledResponse { RetryAfter = Convert.ToInt32(kvp.Value.First()) })
            .IfNone(new ThrottledResponse());
}
