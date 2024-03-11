using FluentAssertions;
using Graphapi.Utils.Models;
using LanguageExt;
using LanguageExt.Common;
using Moq;
using NUnit.Framework;
using Polly.Retry;
using Polly.Testing;
using Serilog;

namespace Graphapi.Utils.Unit.Tests;

[TestFixture]
public class ResiliencePipelinesTests
{
    private Mock<ILogger> _logger;

    [SetUp]
    public void SetUp() => _logger = new Mock<ILogger>();

    [Test]
    public void RetryOnThrottle_ReturnRetryIndefinitely()
    {
        var pipeline = ResiliencePipelines.RetryOnThrottle<int>(_logger.Object).GetPipelineDescriptor();

        pipeline.Strategies[0].Options.Should().BeOfType<RetryStrategyOptions<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<int>>>>>();
        ((RetryStrategyOptions<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<int>>>>)pipeline.Strategies[0].Options!)
            .MaxRetryAttempts
            .Should()
            .Be(int.MaxValue);
    }

    [Test]
    public void RetryOnThrottle_ShouldLogOnRetry()
    {
        var pipeline = ResiliencePipelines.RetryOnThrottle<int>(_logger.Object).GetPipelineDescriptor();

        var onRetry = ((RetryStrategyOptions<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<int>>>>)pipeline.Strategies[0].Options!).OnRetry!;
        onRetry(new OnRetryArguments<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<int>>>>(
            null, 
            new Polly.Outcome<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<int>>>>(), 
            attemptNumber: 1, 
            retryDelay: TimeSpan.FromSeconds(5), 
            TimeSpan.Zero));

        _logger
            .Verify(_ => 
                _.Information("Retrying, attempt {RetryAttempt} in {RetryDelay}", 1, TimeSpan.FromSeconds(5)), 
                Times.Once());
    }

    [Test]
    public void RetryOnThrottle_ShouldNotHandleIfLeft()
    {
        var pipeline = ResiliencePipelines.RetryOnThrottle<int>(_logger.Object);

        pipeline.Execute(token => EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<int>>>.Left(Error.New("")));

        _logger.VerifyNoOtherCalls();
    }

    [Test]
    public void RetryOnThrottle_ShouldNotHandleIfValueReturned()
    {
        var pipeline = ResiliencePipelines.RetryOnThrottle<int>(_logger.Object);

        pipeline.Execute(token => EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<int>>>.Right(new GraphApiPagedResponse<int>()));

        _logger.VerifyNoOtherCalls();
    }

    [Test]
    public async Task RetryOnThrottle_ShouldHandleIfThrottledResponse()
    {
        var pipeline = ResiliencePipelines.RetryOnThrottle<int>(_logger.Object);

        _ = Task.Run(() =>
                pipeline.Execute(
                    token => EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<int>>>.Right(new ThrottledResponse { RetryAfter = 1 })));

        await Task.Delay(1500);

        _logger
            .Verify(_ =>
                _.Information("Retrying, attempt {RetryAttempt} in {RetryDelay}", 1, TimeSpan.FromSeconds(1)),
                Times.Once());
    }
}