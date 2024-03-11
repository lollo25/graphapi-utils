using Graphapi.Utils.Models;
using LanguageExt;
using LanguageExt.Common;
using Polly;

namespace Graphapi.Utils.Services;
public interface IListDownloader<T>
{
    EitherAsync<Error, IEnumerable<T>> DownloadAsync(
        string path,
        ListDownloaderOptions options,
        ResiliencePipeline<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<T>>>> resiliencePipeline,
        CancellationToken cancellationToken);
}