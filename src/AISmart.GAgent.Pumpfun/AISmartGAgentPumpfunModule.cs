using AISmart.Options;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AISmart;

[DependsOn(
    typeof(AISmartApplicationContractsModule)
)]
public class AISmartGAgentPumpfunModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AISmartGAgentPumpfunModule>();
        });
        var configuration = context.Services.GetConfiguration();
        Configure<PumpfunServiceOptions>(configuration.GetSection("PumpfunService")); 

    }
}