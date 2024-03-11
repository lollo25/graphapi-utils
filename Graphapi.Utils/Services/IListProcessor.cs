using Graphapi.Utils.Models;

namespace Graphapi.Utils.Services;
public interface IListProcessor<T>
{
    Task<int> ProcessAsync(ListDownloaderOptions options, CancellationToken cancellationToken);
}