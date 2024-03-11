using Graphapi.Utils.Models;
using LanguageExt;
using LanguageExt.Common;

namespace Graphapi.Utils.Services;
public interface IListFileSystemSaver<T>
{
    EitherAsync<Error, string[]> SaveAsync(
        IEnumerable<T> data, 
        Func<T, string> fileNameProvider, 
        ListDownloaderOptions options);
}