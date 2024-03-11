using FluentAssertions;
using Graphapi.Utils.Models;
using Graphapi.Utils.Services;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.UnitTesting;
using Moq;
using NUnit.Framework;
using Serilog;
using System;

namespace Graphapi.Utils.Unit.Tests.Services;

[TestFixture]
public class LoggedAuthorizationProviderTests
{
    private AuthenticationOptions _authenticationOptions;
    private CancellationToken _cancellationToken;
    private Mock<IAuthorizationProvider> _mockAuthorizationProvider;
    private Mock<ILogger> _mockLogger;
    private LoggedAuthorizationProvider _sut;

    [SetUp]
    public void SetUp()
    {
        _authenticationOptions = new AuthenticationOptions();
        _cancellationToken = new CancellationTokenSource().Token;
        _mockAuthorizationProvider = new Mock<IAuthorizationProvider>();
        _mockLogger = new Mock<ILogger>();
        _sut = new LoggedAuthorizationProvider(
            _mockAuthorizationProvider.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task AuthenticateAsync_InnerOk_ReturnInnerResponse()
    {
        var expectedResult = new ClientCredentialsToken();
        _mockAuthorizationProvider
            .Setup(_ => _.AuthenticateAsync(_authenticationOptions, _cancellationToken))
            .Returns(expectedResult);

        var result = await _sut.AuthenticateAsync(_authenticationOptions, _cancellationToken);

        result.ShouldBeRight(_ => _.Should().Be(expectedResult));
    }

    [Test]
    public async Task AuthenticateAsync_InnerError_LogAndReturnInnerResponse()
    {
        var expectedException = new Exception("Message");
        var expectedError = Error.New(expectedException);
        _mockAuthorizationProvider
            .Setup(_ => _.AuthenticateAsync(_authenticationOptions, _cancellationToken))
            .Returns(expectedError);

        var result = await _sut.AuthenticateAsync(_authenticationOptions, _cancellationToken);

        result.ShouldBeLeft(_ => _.Should().Be(expectedError));
        _mockLogger.Verify(
            _ => _.Error(expectedException, "AuthenticateAsync error: Message"));
    }
}