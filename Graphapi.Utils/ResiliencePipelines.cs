using Graphapi.Utils.Models;
using LanguageExt;
using LanguageExt.Common;
using Polly;
using Serilog;

namespace Graphapi.Utils;
public static class ResiliencePipelines
{
    private const string RetryingLogMessage = "Retrying, attempt {RetryAttempt} in {RetryDelay}";

    public static ResiliencePipeline<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<T>>>> RetryOnThrottle<T>(
        ILogger logger) =>
            new ResiliencePipelineBuilder<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<T>>>>()
                .AddRetry(new Polly.Retry.RetryStrategyOptions<EitherAsync<Error, Either<ThrottledResponse, GraphApiPagedResponse<T>>>>
                {
                    MaxRetryAttempts = int.MaxValue,
                    ShouldHandle = async args =>
                        await args
                            .Outcome
                            .Result
                            .Match(__ =>
                                __.Match(
                                    ___ => false,
                                    tr => true),
                                err => false),
                    DelayGenerator = async args =>
                        await args
                            .Outcome
                            .Result
                            .MatchUnsafe(__ =>
                                __.MatchUnsafe(
                                    ___ => default,
                                    tr => TimeSpan.FromSeconds(tr.RetryAfter)),
                                err => default),
                    OnRetry = args =>
                    {
                        logger.Information(RetryingLogMessage, args.AttemptNumber, args.RetryDelay);
                        return default;
                    }
                })
                .Build();
}
