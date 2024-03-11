using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphapi.Utils.Recursion;
public static class TailRecursion
{
    public static async Task<T> ExecuteAsync<T>(Func<Task<RecursionResult<T>>> func)
    {
        do
        {
            var recursionResult = await func();
            if (recursionResult.IsFinalResult)
                return recursionResult.Result;
            func = recursionResult.NextStepAync;
        } while (true);
    }

    public static Task<RecursionResult<T>> ReturnAsync<T>(T result)
        => RecursionResult<T>.CreateLastAsync(result, null);

    public static Task<RecursionResult<T>> NextAsync<T>(Func<Task<RecursionResult<T>>> nextStep)
        => RecursionResult<T>.CreateNextAsync(default, nextStep);
}
