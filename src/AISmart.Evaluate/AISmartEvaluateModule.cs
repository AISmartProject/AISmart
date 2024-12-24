using AISmart.Evaluate.Service;
using AISmart.Options;
using AISmart.Provider;
using AISmart.Rag;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace AISmart.Evaluate;

[DependsOn(
    typeof(AISmartSimpleRagModule)
)]
public class AISmartEvaluateModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        // Configure<RagOptions>(configuration.GetSection("EvaluateRag"));
        // context.Services.AddSingleton<IRagProvider, RagProvider>();
        // context.Services.AddSingleton<IAISmartEvaluateService, AISmartEvaluateService>();
        // Configure<RagOptions>("EvaluateRag", configuration.GetSection("Evaluate:Rag"));
    }
}