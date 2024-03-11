using Graphapi.Utils.Models;
using LanguageExt;
using LanguageExt.Common;
using System.IO.Abstractions;
using System.Text.Json;

namespace Graphapi.Utils.Services;
public class ListFileSystemSaver<T> : IListFileSystemSaver<T>
{
    private readonly IFileSystem _fileSystem;

    public ListFileSystemSaver(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    public EitherAsync<Error, string[]> SaveAsync(
        IEnumerable<T> data,
        Func<T, string> fileNameProvider,
        ListDownloaderOptions options) =>
            Prelude.TryAsync(async () =>
                {
                    var directory = _fileSystem.Directory.CreateDirectory(options.Directory);
                    return await Task.WhenAll(
                        data
                        .Map(i => (i, path: $"{_fileSystem.Path.Combine(directory.FullName, fileNameProvider(i))}"))
                        .Map(async t =>
                        {
                            await _fileSystem.File.WriteAllBytesAsync(
                                t.path,
                                JsonSerializer.SerializeToUtf8Bytes(t.i));
                            return t.path;
                        }));
                })
            .ToEither();
}