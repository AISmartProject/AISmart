using System;
using System.Threading.Tasks;
using AISmart.Provider;
using AISmart.Rag;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace AISmart.Evaluate.Service;

public class AISmartEvaluateService : IAISmartEvaluateService, ISingletonDependency
{
    private readonly ILogger<AISmartEvaluateService> _logger;
    private readonly IRagProvider _ragProvider;
    // private readonly IOptionsMonitor<EvaluateOptions> _evaluateOptions;
    private readonly IRagProviderFactory _ragProviderFactory;

    public AISmartEvaluateService(ILogger<AISmartEvaluateService> logger, IRagProviderFactory ragProviderFactory)
    {
        _logger = logger;
        _ragProvider = ragProviderFactory.GetProvider("EvaluateRag");
    }

    public async Task EvaluateAsync(string task, string result)
    {
        throw new NotImplementedException();
    }
    
    public async Task AddExceptionMessageAsync(string task, string exceptionMessage)
    {
        var text =
            $"""
             Task is: {task} 
             Exception {exceptionMessage} was caught during execution, please pay attention!
             """;
        await _ragProvider.StoreTextAsync(text);
    }
    
    public async Task<string> GetAdviceAsync(string task)
    {
        return await _ragProvider.RetrieveAnswerAsync(task);
    }
}