using FluentAssertions;
using Graphapi.Utils.Models;
using Graphapi.Utils.Services;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.UnitTesting;
using Moq;
using NUnit.Framework;
using Polly;

namespace Graphapi.Utils.Unit.Tests.Services;

[TestFixture]
public class ListDownloaderTests
{
    private ResiliencePipeline<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<int>>>> _emptyResiliencePipeline;
    private string _path;
    private string _accessToken;
    private Mock<IAuthorizationProvider> _mockAuthorizationProvider;
    private Mock<IGraphApiClient<int>> _mockGraphApiClient;
    private ListDownloaderOptions _options;
    private CancellationTokenSource _cancellationTokenSource;
    private ListDownloader<int> _sut;

    [SetUp]
    public void SetUp()
    {
        _emptyResiliencePipeline = ResiliencePipeline<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<int>>>>.Empty;
        _path = "PATH";
        _accessToken = "AT";
        _mockAuthorizationProvider = new Mock<IAuthorizationProvider>();
        _mockGraphApiClient = new Mock<IGraphApiClient<int>>();
        _options = new ListDownloaderOptions { AppId = "aid", AppSecret = "as", Tenant = "t", PageSize = 10 };
        _cancellationTokenSource = new CancellationTokenSource();
        _sut = new ListDownloader<int>(
            _mockAuthorizationProvider.Object,
            _mockGraphApiClient.Object);
    }

    [Test]
    public async Task DownloadAsync_AuthenticationFails_ReturnError()
    {
        var authError = Error.New("ERR");
        _mockAuthorizationProvider
            .Setup(_ => _.AuthenticateAsync(_options, _cancellationTokenSource.Token))
            .Returns(authError);

        var result = await _sut.DownloadAsync(_path, _options, _emptyResiliencePipeline, _cancellationTokenSource.Token);

        result.ShouldBeLeft(err => err.Should().Be(authError));
    }

    [Test]
    public async Task DownloadAsync_FirstGetDoesNotHaveNextPage_ReturnData()
    {
        _mockAuthorizationProvider
            .Setup(_ => _.AuthenticateAsync(_options, _cancellationTokenSource.Token))
            .Returns(new ClientCredentialsToken { AccessToken = _accessToken });
        _mockGraphApiClient
            .Setup(_ => _.GetAsync("https://graph.microsoft.com/v1.0/PATH?$top=10", _accessToken, _cancellationTokenSource.Token))
            .Returns(Either<ThrottledResponse, GraphApiPagedResponse<int>>.Right(new GraphApiPagedResponse<int> { Value = new[] { 1, 2 } }));

        var result = await _sut.DownloadAsync(_path, _options, _emptyResiliencePipeline, _cancellationTokenSource.Token);

        result.ShouldBeRight(_ => _.Should().BeEquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public async Task DownloadAsync_FirstGetHasNextPage_ShouldGetAgain()
    {
        _mockAuthorizationProvider
            .Setup(_ => _.AuthenticateAsync(_options, _cancellationTokenSource.Token))
            .Returns(new ClientCredentialsToken { AccessToken = _accessToken });
        _mockGraphApiClient
            .Setup(_ => _.GetAsync("https://graph.microsoft.com/v1.0/PATH?$top=10", _accessToken, _cancellationTokenSource.Token))
            .Returns(Either<ThrottledResponse, GraphApiPagedResponse<int>>.Right(new GraphApiPagedResponse<int>
            {
                Value = new[] { 1, 2 },
                Next = new Uri("http://nextpage")
            }));
        _mockGraphApiClient
            .Setup(_ => _.GetAsync("http://nextpage/", _accessToken, _cancellationTokenSource.Token))
            .Returns(Either<ThrottledResponse, GraphApiPagedResponse<int>>.Right(new GraphApiPagedResponse<int>
            {
                Value = new[] { 1, 2 }
            }));

        await _sut.DownloadAsync(_path, _options, _emptyResiliencePipeline, _cancellationTokenSource.Token);

        _mockGraphApiClient
           .Verify(_ => _.GetAsync("http://nextpage/", _accessToken, _cancellationTokenSource.Token), Times.Once());
    }

    [Test]
    public async Task DownloadAsync_FirstGetHasNextPage_ShouldGetAgainAndReturnAllData()
    {
        _mockAuthorizationProvider
                    .Setup(_ => _.AuthenticateAsync(_options, _cancellationTokenSource.Token))
                    .Returns(new ClientCredentialsToken { AccessToken = _accessToken });
        _mockGraphApiClient
            .Setup(_ => _.GetAsync("https://graph.microsoft.com/v1.0/PATH?$top=10", _accessToken, _cancellationTokenSource.Token))
            .Returns(Either<ThrottledResponse, GraphApiPagedResponse<int>>.Right(new GraphApiPagedResponse<int>
            {
                Value = new[] { 1, 2 },
                Next = new Uri("http://nextpage")
            }));
        _mockGraphApiClient
            .Setup(_ => _.GetAsync("http://nextpage/", _accessToken, _cancellationTokenSource.Token))
            .Returns(Either<ThrottledResponse, GraphApiPagedResponse<int>>.Right(new GraphApiPagedResponse<int>
            {
                Value = new[] { 3, 4 }
            }));

        var result = await _sut.DownloadAsync(_path, _options, _emptyResiliencePipeline, _cancellationTokenSource.Token);

        _mockGraphApiClient
           .Verify(_ => _.GetAsync("http://nextpage/", _accessToken, _cancellationTokenSource.Token), Times.Once());
        result.ShouldBeRight(_ => _.Should().BeEquivalentTo(new[] { 1, 2, 3, 4 }));
    }

    [Test]
    public async Task DownloadAsync_GetFails_ReturnError()
    {
        var getError = Error.New("ERR");
        _mockAuthorizationProvider
                    .Setup(_ => _.AuthenticateAsync(_options, _cancellationTokenSource.Token))
                    .Returns(new ClientCredentialsToken { AccessToken = _accessToken });
        _mockGraphApiClient
            .Setup(_ => _.GetAsync("https://graph.microsoft.com/v1.0/PATH?$top=10", _accessToken, _cancellationTokenSource.Token))
            .Returns(Either<ThrottledResponse, GraphApiPagedResponse<int>>.Right(new GraphApiPagedResponse<int>
            {
                Value = new[] { 1, 2 },
                Next = new Uri("http://nextpage")
            }));
        _mockGraphApiClient
            .Setup(_ => _.GetAsync("http://nextpage/", _accessToken, _cancellationTokenSource.Token))
            .Returns(getError);

        var result = await _sut.DownloadAsync(_path, _options, _emptyResiliencePipeline, _cancellationTokenSource.Token);

        _mockGraphApiClient
           .Verify(_ => _.GetAsync("http://nextpage/", _accessToken, _cancellationTokenSource.Token), Times.Once());
        result.ShouldBeLeft(err => err.Should().Be(getError));
    }
}