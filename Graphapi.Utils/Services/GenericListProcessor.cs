using Graphapi.Utils.Models;
using Serilog;

namespace Graphapi.Utils.Services;
public class GenericListProcessor<T> : IListProcessor<T>
{
    private const string GroupsCountLogFormat = "Groups are {GroupsCount}";
    private const string GroupPathLogformat = "Group saved to {GroupPath}";
    private const string ErrorLogFormat = "An error occured with code {Code} and message {Message}";
    private readonly Dictionary<Func<bool>, string> _typesAddress = new()
    {
        { () => typeof(T) == typeof(GraphApiGroup), "groups" }
    };
    private readonly Dictionary<Func<bool>, Func<T, string>> _typesFilenames = new()
    {
        { () => typeof(T) == typeof(GraphApiGroup), (T g) => $"{(g as GraphApiGroup)!.DisplayName}.json" }
    };

    private readonly IListDownloader<T> _downloader;
    private readonly IListFileSystemSaver<T> _saver;
    private readonly ILogger _logger;

    public GenericListProcessor(
        IListDownloader<T> downloader,
        IListFileSystemSaver<T> saver,
        ILogger logger)
    {
        _downloader = downloader;
        _saver = saver;
        _logger = logger;
    }

    public async Task<int> ProcessAsync(
        ListDownloaderOptions options,
        CancellationToken cancellationToken)
    {
        var address = _typesAddress.First(_ => _.Key()).Value;
        var filenameCreator = _typesFilenames.First(_ => _.Key()).Value;
        return
            await _downloader.DownloadAsync(address, options, ResiliencePipelines.RetryOnThrottle<T>(_logger), cancellationToken)
                .Bind(_ => _saver.SaveAsync(_, filenameCreator, options))
                .Match(
                    list =>
                    {
                        _logger.Information(GenericListProcessor<T>.GroupsCountLogFormat, list.Length());
                        foreach (var item in list)
                        {
                            _logger.Information(GenericListProcessor<T>.GroupPathLogformat, item);
                        }
                        return 0;
                    },
                    err =>
                    {
                        _logger.Error(GenericListProcessor<T>.ErrorLogFormat, err.Code, err.Message);
                        return 1;
                    });
    }
}
