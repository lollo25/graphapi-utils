using Graphapi.Utils.Models;
using LanguageExt;
using LanguageExt.Common;

namespace Graphapi.Utils.Services;
public interface IAuthorizationProvider
{
    EitherAsync<Error, ClientCredentialsToken> AuthenticateAsync(AuthenticationOptions authenticationOptions, CancellationToken cancellationToken);
}