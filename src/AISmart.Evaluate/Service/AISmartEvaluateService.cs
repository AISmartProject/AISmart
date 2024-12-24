using System;
using System.Threading.Tasks;
using AISmart.Rag;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace AISmart.Evaluate.Service;

public class AISmartEvaluateService : IAISmartEvaluateService, ISingletonDependency
{
    private readonly ILogger<AISmartEvaluateService> _logger;
    private readonly IRagProvider _ragProvider;

    public AISmartEvaluateService(ILogger<AISmartEvaluateService> logger, IRagProvider ragProvider)
    {
        _logger = logger;
        _ragProvider = ragProvider;
    }

    public async Task EvaluateAsync(string task, string result)
    {
        throw new NotImplementedException();
    }
    
    public async Task AddExceptionMessageAsync(string task, string exceptionMessage)
    {
        var text =
            $"Task is: {task}, and exception {exceptionMessage} was caught during execution, please pay attention";
        await _ragProvider.StoreTextAsync(text);
    }
    
    public async Task<string> GetAdviceAsync(string task)
    {
        return await _ragProvider.RetrieveAnswerAsync(task);
    }
}