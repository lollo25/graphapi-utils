using FluentAssertions;
using Flurl;
using Graphapi.Groups.Retriever.Models;
using Graphapi.Groups.Retriever.Services;
using LanguageExt.Common;
using LanguageExt.UnitTesting;
using Moq;
using Moq.Contrib.HttpClient;
using NUnit.Framework;
using System;
using System.CommandLine;
using System.Net;
using System.Net.Http;

namespace Graphapi.Groups.Retriever.Unit.Tests.Services;

[TestFixture]
public class GraphApiAuthorizationProviderTests
{
    private const string ExpectedRequestUrl = "http://test.com/tenant/oauth2/v2.0/token";
    private AuthenticationOptions _authenticationOptions;
    private CancellationTokenSource _cancellationTokenSource;
    private Mock<HttpMessageHandler> _mockHttpHandler;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private GraphApiAuthorizationProvider _sut;

    [SetUp]
    public void SetUp()
    {
        _authenticationOptions = new AuthenticationOptions { Tenant = "tenant", AppId = "appid", AppSecret = "appsecret" };
        _cancellationTokenSource = new CancellationTokenSource();
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpClientFactory
            .Setup(_ => _.CreateClient("MSFT_LOGIN_CLIENT"))
            .Returns(() =>
            {
                var client = _mockHttpHandler.CreateClient();
                client.BaseAddress = new Uri("http://test.com");
                return client;
            });
        _sut = new GraphApiAuthorizationProvider(
            _mockHttpClientFactory.Object);
    }

    [Test]
    public async Task AuthenticateAsync_SendExpectedRequest()
    {
        await _sut.AuthenticateAsync(_authenticationOptions, _cancellationTokenSource.Token);

        _mockHttpHandler.VerifyRequest(
            HttpMethod.Post,
            ExpectedRequestUrl,
            r => r.Content!.Headers.TryGetValues("Content-Type", out var ct) && ct.First() == "application/x-www-form-urlencoded", Times.Once());
        _mockHttpHandler.VerifyRequest(
            ExpectedRequestUrl,
            async r => (await ExtractDictionary(r)).TryGetValue("client_id", out var val) && val == _authenticationOptions.AppId, Times.Once());
        _mockHttpHandler.VerifyRequest(
            ExpectedRequestUrl,
            async r => (await ExtractDictionary(r)).TryGetValue("client_secret", out var val) && val == _authenticationOptions.AppSecret, Times.Once());
        _mockHttpHandler.VerifyRequest(
            ExpectedRequestUrl,
            async r => (await ExtractDictionary(r)).TryGetValue("grant_type", out var val) && val == "client_credentials", Times.Once());
    }

    [Test]
    public async Task AuthenticateAsync_RequestFails_ReturnErrorWithException()
    {
        _mockHttpHandler
            .SetupRequest(ExpectedRequestUrl)
            .ReturnsResponse(HttpStatusCode.InternalServerError);

        var result = await _sut.AuthenticateAsync(_authenticationOptions, _cancellationTokenSource.Token);

        result.ShouldBeLeft(err => err.Exception.ShouldBeSome(ex => ex.Should().BeOfType(typeof(HttpRequestException))));
    }
    [Test]
    public async Task AuthenticateAsync_DeserializationFails_ReturnErrorWithException()
    {
        _mockHttpHandler
            .SetupRequest(ExpectedRequestUrl)
            .ReturnsResponse(HttpStatusCode.OK, res => res.Content = new StringContent(null));

        var result = await _sut.AuthenticateAsync(_authenticationOptions, _cancellationTokenSource.Token);

        result.ShouldBeLeft(err => err.Exception.ShouldBeSome(ex => ex.Should().BeOfType(typeof(ArgumentNullException))));
    }
    [Test]
    public async Task AuthenticateAsync_Successful_ReturnResponseModel()
    {
        _mockHttpHandler
            .SetupRequest(ExpectedRequestUrl)
            .ReturnsResponse(HttpStatusCode.OK, res => res.Content = new StringContent("{\"access_token\": \"token\", \"token_type\": \"type\", \"expires_in\": 600}"));

        var result = await _sut.AuthenticateAsync(_authenticationOptions, _cancellationTokenSource.Token);

        result.ShouldBeRight(ah => ah.Should().BeEquivalentTo(new ClientCredentialsToken { TokenType = "type", AccessToken = "token", ExpiresIn = 600 }));
    }

    private static async Task<Dictionary<string, string>> ExtractDictionary(HttpRequestMessage r)
    {
        var json = await r.Content.ReadAsStringAsync();
        return Url.ParseQueryParams(json).ToDictionary(_ => _.Name, _ => _.Value.ToString())!;
    }
}