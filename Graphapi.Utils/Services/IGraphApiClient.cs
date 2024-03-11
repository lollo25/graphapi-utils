using Graphapi.Utils.Models;
using LanguageExt;
using LanguageExt.Common;

namespace Graphapi.Utils.Services;
public interface IGraphApiClient<T>
{
    EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<T>>> GetAsync(
        string address, string accessToken, CancellationToken cancellationToken);
}