using AISmart.Options;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AISmart;

[DependsOn(
    typeof(AISmartApplicationContractsModule)
    )]
public class AISmartGAgentTwitterModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AISmartGAgentTwitterModule>();
        });
        var configuration = context.Services.GetConfiguration();
        Configure<TwitterOptions>(configuration.GetSection("Twitter")); 

    }
}
