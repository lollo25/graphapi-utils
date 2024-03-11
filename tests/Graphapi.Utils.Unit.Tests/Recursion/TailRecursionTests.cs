using FluentAssertions;
using Graphapi.Utils.Recursion;
using NUnit.Framework;

namespace Graphapi.Utils.Unit.Tests.Recursion;

[TestFixture]
public class TailRecursionTests
{
    [Test]
    public async Task ExecuteAsync_ShouldCallFunc()
    {
        var called = false;
        Func<Task<RecursionResult<LanguageExt.Unit>>> callback = () =>
        {
            called = true;
            return TailRecursion.ReturnAsync(LanguageExt.Unit.Default);
        };

        var _ = await TailRecursion.ExecuteAsync(callback);

        called
            .Should()
            .BeTrue();
    }
    [Test]
    public async Task ExecuteAsync_ShouldCallNext()
    {
        var result = await TailRecursion.ExecuteAsync(() => TestFuncAsync(0));

        result
            .Should()
            .Be(1);
    }

    private Task<RecursionResult<int>> TestFuncAsync(int current) => 
        current == 0 ?
            TailRecursion.NextAsync(() => TestFuncAsync(current + 1)) :
            TailRecursion.ReturnAsync(current);
}