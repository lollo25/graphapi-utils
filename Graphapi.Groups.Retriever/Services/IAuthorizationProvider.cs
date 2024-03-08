using Graphapi.Groups.Retriever.Models;
using LanguageExt;
using LanguageExt.Common;

namespace Graphapi.Groups.Retriever.Services;
public interface IAuthorizationProvider
{
    EitherAsync<Error, ClientCredentialsToken> AuthenticateAsync(AuthenticationOptions authenticationOptions, CancellationToken cancellationToken);
}