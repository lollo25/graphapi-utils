using FluentAssertions;
using Graphapi.Utils.Models;
using Graphapi.Utils.Services;
using LanguageExt;
using LanguageExt.Common;
using Moq;
using NUnit.Framework;
using Polly;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Graphapi.Utils.Unit.Tests.Services;

[TestFixture]
public class GenericListProcessorTests
{
    private ListDownloaderOptions _options;
    private CancellationTokenSource _cancellationTokenSource;
    private Mock<IListDownloader<GraphApiGroup>> _mockListDownloader;
    private Mock<IListFileSystemSaver<GraphApiGroup>> _mockListFileSystemSaver;
    private Mock<ILogger> _mockLogger;
    private GenericListProcessor<GraphApiGroup> _sut;

    [SetUp]
    public void SetUp()
    {
        _options = new ListDownloaderOptions
        {
            AppId = "aid",
            AppSecret = "as",
            Tenant = "t",
            PageSize = 10,
            GraphApiRootUrl = "https://graph.microsoft.com",
            GraphApiVersion = "v1.0"
        };
        _cancellationTokenSource = new CancellationTokenSource();
        _mockListDownloader = new Mock<IListDownloader<GraphApiGroup>>();
        _mockListFileSystemSaver = new Mock<IListFileSystemSaver<GraphApiGroup>>();
        _mockLogger = new Mock<ILogger>();
        _sut = new GenericListProcessor<GraphApiGroup>(
            _mockListDownloader.Object,
            _mockListFileSystemSaver.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task ProcessAsync_DownloaderFails_LogAndReturn1()
    {
        var error = Error.New(401, "Unauthorized");
        _mockListDownloader
            .Setup(_ => _.DownloadAsync(
                "groups",
                _options,
                It.IsAny<ResiliencePipeline<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<GraphApiGroup>>>>>(),
                _cancellationTokenSource.Token))
            .Returns(error);

        var result = await _sut.ProcessAsync(_options, _cancellationTokenSource.Token);

        _mockLogger
            .Verify(_ => _.Error("An error occured with code {Code} and message {Message}", 401, "Unauthorized"),
            Times.Once());
        result.Should().Be(1);
    }

    [Test]
    public async Task ProcessAsync_SaverFails_LogAndReturn1()
    {
        var error = Error.New(400, "BAD REQUEST");
        Func<GraphApiGroup, string> filenameProvider = _ => string.Empty;
        var downloaderResult = new GraphApiGroup[] { new GraphApiGroup() };
        _mockListDownloader
            .Setup(_ => _.DownloadAsync(
                "groups",
                _options,
                It.IsAny<ResiliencePipeline<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<GraphApiGroup>>>>>(),
                _cancellationTokenSource.Token))
            .Returns(downloaderResult);
        _mockListFileSystemSaver
            .Setup(_ => _.SaveAsync(downloaderResult, It.IsAny<Func<GraphApiGroup, string>>(), _options))
            .Callback<IEnumerable<GraphApiGroup>, Func<GraphApiGroup, string>, ListDownloaderOptions>((_, fn, __) => filenameProvider = fn)
            .Returns(error);

        var result = await _sut.ProcessAsync(_options, _cancellationTokenSource.Token);

        filenameProvider(new GraphApiGroup { DisplayName = "name" }).Should().Be("name.json");
        _mockLogger
            .Verify(_ => _.Error("An error occured with code {Code} and message {Message}", 400, "BAD REQUEST"),
            Times.Once());
        result.Should().Be(1);
    }

    [Test]
    public async Task ProcessAsync_AllOk_LogAndReturn()
    {
        var downloaderResult = new GraphApiGroup[] { new GraphApiGroup() };
        _mockListDownloader
            .Setup(_ => _.DownloadAsync(
                "groups",
                _options,
                It.IsAny<ResiliencePipeline<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<GraphApiGroup>>>>>(),
                _cancellationTokenSource.Token))
            .Returns(downloaderResult);
        _mockListFileSystemSaver
            .Setup(_ => _.SaveAsync(downloaderResult, It.IsAny<Func<GraphApiGroup, string>>(), _options))
            .Returns(new[] { "f1" });

        var result = await _sut.ProcessAsync(_options, _cancellationTokenSource.Token);
        _mockLogger
            .Verify(_ => _.Information("Groups are {GroupsCount}", 1),
            Times.Once());
        _mockLogger
            .Verify(_ => _.Information("Group saved to {GroupPath}", "f1"),
            Times.Once());
        result.Should().Be(0);
    }
}