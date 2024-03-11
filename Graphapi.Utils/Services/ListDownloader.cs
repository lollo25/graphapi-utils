using Graphapi.Utils.Models;
using Graphapi.Utils.Recursion;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.UnsafeValueAccess;
using Polly;

namespace Graphapi.Utils.Services;
public class ListDownloader<T> : IListDownloader<T>
{
    private readonly IAuthorizationProvider _authorizationProvider;
    private readonly IGraphApiClient<T> _client;

    public ListDownloader(
        IAuthorizationProvider authorizationProvider,
        IGraphApiClient<T> client)
    {
        _authorizationProvider = authorizationProvider;
        _client = client;
    }
    public EitherAsync<Error, IEnumerable<T>> DownloadAsync(
        string path,
        ListDownloaderOptions options,
        ResiliencePipeline<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<T>>>> resiliencePipeline,
        CancellationToken cancellationToken)
    {
        return _authorizationProvider.AuthenticateAsync(options, cancellationToken)
            .Bind(_ => LoopThroughPages(path, options, _, resiliencePipeline, cancellationToken));
    }

    private EitherAsync<Error, IEnumerable<T>> LoopThroughPages(
        string path,
        ListDownloaderOptions options,
        ClientCredentialsToken clientCredentials,
        ResiliencePipeline<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<T>>>> resiliencePipeline,
        CancellationToken cancellationToken)
    {
        EitherAsync<Error, GraphApiPagedResponse<T>> GetAsync(
            string address) =>
                resiliencePipeline.Execute(token =>
                    _client.GetAsync(address, clientCredentials.AccessToken, cancellationToken),
                    cancellationToken).Map(_ => _.ValueUnsafe());
        return 
            GetAsync($"{Constants.GraphApiRootUrl}/{Constants.GraphApiVersion}/{path}?$top={options.Top}")
                .Bind(_ =>
                    TailRecursion
                        .ExecuteAsync(() => Execute(_, GetAsync))
                        .ToAsync());  
    }

    private Task<RecursionResult<Either<Error, IEnumerable<T>>>> Execute(
        GraphApiPagedResponse<T> graphApiPagedResponse, 
        Func<string, EitherAsync<Error, GraphApiPagedResponse<T>>> getAsync)
    {
        return graphApiPagedResponse.Next == default
            ? TailRecursion.ReturnAsync((Either<Error, IEnumerable<T>>)graphApiPagedResponse.Value)
            : getAsync(graphApiPagedResponse.Next.ToString())
                .MatchAsync(
                    _ => Execute(_ with { Value = graphApiPagedResponse.Value.Concat(_.Value).ToArray()}, getAsync),
                    err => TailRecursion.ReturnAsync((Either<Error, IEnumerable<T>>)err));
    }

}
