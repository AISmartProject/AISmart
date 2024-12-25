using AISmart.Options;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace AISmart;

[DependsOn(
    typeof(AISmartApplicationModule),
    typeof(AbpEventBusModule)
)]
public class AISmartAgentConfigModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        base.ConfigureServices(context);
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<AISmartApplicationModule>(); });
        var configuration = context.Services.GetConfiguration();
        Configure<AgentNetworkConfigOptions>(configuration.GetSection("NetWork"));   
    }
}