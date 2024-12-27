
using AiSmart.GAgent.TestAgent;
using AISmart.Options;
using AISmart.Service;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Volo.Abp.AutoMapper;
using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace AISmart;

[DependsOn(
    typeof(AISmartApplicationModule),
    typeof(AbpEventBusModule),
    typeof(AISmartOrleansTestBaseModule),
    typeof(AISmartGAgentTestAgentModule),
    typeof(AISmartGAgentTestAgentModule)
)]
public class AISmartNameContestTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        base.ConfigureServices(context);
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<AISmartGAgentTestAgentModule>(); });

        var configuration = context.Services.GetConfiguration();
        
        Configure<NameContestOptions>(configuration.GetSection("NameContest"));
        Configure<MicroAIOptions>(configuration.GetSection("AutogenConfig"));

        
        context.Services.AddSingleton<INamingContestService, NamingContestService>();

      
    }
}