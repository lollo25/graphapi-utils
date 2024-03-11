using Graphapi.Utils.Models;
using LanguageExt;
using LanguageExt.Common;
using Serilog;

namespace Graphapi.Utils.Services;
public class LoggedAuthorizationProvider : IAuthorizationProvider
{
    private readonly IAuthorizationProvider _inner;
    private readonly ILogger _logger;

    public LoggedAuthorizationProvider(IAuthorizationProvider inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }
    public EitherAsync<Error, ClientCredentialsToken> AuthenticateAsync(AuthenticationOptions authenticationOptions, CancellationToken cancellationToken)
    {
        return _inner.AuthenticateAsync(authenticationOptions, cancellationToken)
            .MapLeft(
                err =>
                {
                    _logger.Error(err.ToException(), $"{nameof(AuthenticateAsync)} error: {err.Message}");
                    return err;
                });
    }
}
