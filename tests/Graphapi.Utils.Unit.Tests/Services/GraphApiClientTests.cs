using FluentAssertions;
using Graphapi.Utils.Services;
using LanguageExt.UnitTesting;
using Moq;
using Moq.Contrib.HttpClient;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Graphapi.Utils.Unit.Tests.Services;

[TestFixture]
public class GraphApiClientTests
{
    private const string ExpectedRequestUrl = "http://test.com/getasync";
    private string _address;
    private string _accessToken;
    private CancellationTokenSource _cancellationTokenSource;
    private Mock<HttpMessageHandler> _mockHttpHandler;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private GraphApiClient<int> _sut;

    [SetUp]
    public void SetUp()
    {
        _address = "ADDRESS";
        _accessToken = "AT";
        _cancellationTokenSource = new CancellationTokenSource();
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpClientFactory
            .Setup(_ => _.CreateClient("MSFT_GRAPH_API_CLIENT"))
            .Returns(_mockHttpHandler.CreateClient());
        _sut = new GraphApiClient<int>(
            _mockHttpClientFactory.Object);
    }

    [Test]
    public async Task GetPagedAsync_HttpGetFails_ReturnError()
    {
        _cancellationTokenSource.Cancel();

        var result = await _sut.GetPagedAsync(ExpectedRequestUrl, _accessToken, _cancellationTokenSource.Token);

        result.ShouldBeLeft(err => err.ToException().Should().BeOfType<InvalidOperationException>());
    }

    [Test]
    public async Task GetPagedAsync_HttpGet429_ReturnThrottledResponse()
    {
        _mockHttpHandler
            .SetupRequest(ExpectedRequestUrl)
            .ReturnsResponse(HttpStatusCode.TooManyRequests, res => res.Headers.Add("Retry-After", "5"));

        var result = await _sut.GetPagedAsync(ExpectedRequestUrl, _accessToken, _cancellationTokenSource.Token);

        result.ShouldBeRight(_ => _.ShouldBeLeft(tr => tr.RetryAfter.Should().Be(5)));
    }
    [Test]
    public async Task GetPagedAsync_HttpGetNotSuccessful_DeserializationFails_ReturnGenericError()
    {
        _mockHttpHandler
            .SetupRequest(ExpectedRequestUrl)
            .ReturnsResponse(HttpStatusCode.BadRequest);

        var result = await _sut.GetPagedAsync(ExpectedRequestUrl, _accessToken, _cancellationTokenSource.Token);

        result.ShouldBeLeft(err =>
        {
            err.Code.Should().Be((int)HttpStatusCode.BadRequest);
            err.Message.Should().Be("The request was not successful and the serialization of the error model fails.");
        });
    }
    [Test]
    public async Task GetPagedAsync_HttpGetNotSuccessful_ReturnErrorWithMessage()
    {
        _mockHttpHandler
            .SetupRequest(ExpectedRequestUrl)
            .ReturnsResponse(HttpStatusCode.BadRequest, res => res.Content = new StringContent("{\"error\": {\"code\": \"string\",\"message\":\"ERROR_MESSAGE\",\"innererror\":{\"code\":\"string\"},\"details\":[]}}", MediaTypeHeaderValue.Parse("application/json")));

        var result = await _sut.GetPagedAsync(ExpectedRequestUrl, _accessToken, _cancellationTokenSource.Token);

        result.ShouldBeLeft(err =>
        {
            err.Code.Should().Be((int)HttpStatusCode.BadRequest);
            err.Message.Should().Be("The request was not successful with message: ERROR_MESSAGE");
        });
    }
    [Test]
    public async Task GetPagedAsync_HttpGet200_DeserializationFails_ReturnGenericError()
    {
        _mockHttpHandler
            .SetupRequest(ExpectedRequestUrl)
            .ReturnsResponse(HttpStatusCode.OK);

        var result = await _sut.GetPagedAsync(ExpectedRequestUrl, _accessToken, _cancellationTokenSource.Token);

        result.ShouldBeLeft(err =>
        {
            err.Code.Should().Be((int)HttpStatusCode.OK);
            err.Message.Should().Be("The request was successful but the serialization of the error model fails.");
        });
    }
    [Test]
    public async Task GetPagedAsync_HttpGet200_ReturnData()
    {
        _mockHttpHandler
            .SetupRequest(ExpectedRequestUrl)
            .ReturnsResponse(HttpStatusCode.OK, res => res.Content = new StringContent("{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#groups\",\"@odata.nextLink\":\"https://graph.microsoft.com/v1.0/groups?$top=1&$skiptoken=RFNwdAIAAQAAACpHcm91cF8xODQwYzM3My1jMjI5LTQ3Y2MtOGYzMC0yM2E5YjBhOTAzNGMqR3JvdXBfMTg0MGMzNzMtYzIyOS00N2NjLThmMzAtMjNhOWIwYTkwMzRjAAAAAAAAAAAAAAA\",\"value\":[1,2]}", MediaTypeHeaderValue.Parse("application/json")));

        var result = await _sut.GetPagedAsync(ExpectedRequestUrl, _accessToken, _cancellationTokenSource.Token);

        result
            .ShouldBeRight(_ =>
                _.ShouldBeRight(m =>
                    m
                        .Value
                        .Should()
                        .BeEquivalentTo(new[] { 1, 2 })));
        result
            .ShouldBeRight(_ => 
                _.ShouldBeRight(m => 
                    m
                        .Next
                        .Should()
                        .Be("https://graph.microsoft.com/v1.0/groups?$top=1&$skiptoken=RFNwdAIAAQAAACpHcm91cF8xODQwYzM3My1jMjI5LTQ3Y2MtOGYzMC0yM2E5YjBhOTAzNGMqR3JvdXBfMTg0MGMzNzMtYzIyOS00N2NjLThmMzAtMjNhOWIwYTkwMzRjAAAAAAAAAAAAAAA")));
    }
}
