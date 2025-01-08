using AISmart.Options;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AISmart;

[DependsOn(
    typeof(AISmartApplicationContractsModule)
    )]
public class AISmartGAgentMicroAIModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AISmartGAgentMicroAIModule>();
        });
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MicroAIOptions>(configuration.GetSection("AutogenConfig"));
    }
}
