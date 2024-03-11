using System;
using System.Threading.Tasks;

namespace Graphapi.Utils.Recursion;

public class RecursionResult<T>
{
    private RecursionResult(bool isFinalResult,
                            T result,
                            Func<Task<RecursionResult<T>>> nextStep)
    {
        IsFinalResult = isFinalResult;
        Result = result;
        NextStepAync = nextStep;
    }

    public bool IsFinalResult { get; private set; }
    public T Result { get; private set; }
    public Func<Task<RecursionResult<T>>> NextStepAync { get; private set; }

    public static Task<RecursionResult<T>> CreateNextAsync(T result, Func<Task<RecursionResult<T>>> nextStep) => 
        Task.FromResult(new RecursionResult<T>(false, result, nextStep));

    public static Task<RecursionResult<T>> CreateLastAsync(T result, Func<Task<RecursionResult<T>>> nextStep) => 
        Task.FromResult(new RecursionResult<T>(true, result, nextStep));
}
