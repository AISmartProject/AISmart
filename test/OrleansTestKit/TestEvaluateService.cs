using AISmart.Evaluate.Service;

namespace Orleans.TestKit;

public class TestEvaluateService : IAISmartEvaluateService
{
    private string taskWithException;
    private string exceptionMessage;
    public async Task EvaluateAsync(string task, string result)
    {
        return;
    }
    
    public async Task AddExceptionMessageAsync(string task, string exception)
    {
        taskWithException = task;
        exceptionMessage = exception;
    }
    
    public async Task<string> GetAdviceAsync(string task)
    {
        if (taskWithException == task)
        {
            return await Task.FromResult(exceptionMessage);
        }
        return await Task.FromResult("");
    }
}