using Graphapi.Utils.Models;
using LanguageExt;
using LanguageExt.Common;
using System.Text.Json;

namespace Graphapi.Utils.Services;
public class ListFileSystemSaver<T> : IListFileSystemSaver<T>
{
    public EitherAsync<Error, IEnumerable<string>> SaveAsync(
        IEnumerable<T> data,
        Func<T, string> fileNameProvider,
        ListDownloaderOptions options) =>
            Prelude.Try(() =>
                {
                    var directory = Directory.CreateDirectory(options.Directory);
                    return data
                        .Map(i => (i, path: $"{Path.Combine(directory.FullName, fileNameProvider(i))}"))
                        .Map(t =>
                        {
                            File.WriteAllBytes(
                                t.path,
                                JsonSerializer.SerializeToUtf8Bytes(t.i));
                            return t.path;
                        });
                })
            .ToEither(Error.New)
            .ToAsync();
}