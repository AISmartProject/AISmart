using AISmart;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AISmart.Options;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AiSmart.GAgent.TestAgent;

[DependsOn(
    typeof(AISmartApplicationContractsModule),
    typeof(AISmartGAgentMicroAIModule)
)]
public class AISmartGAgentTestAgentModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AISmartGAgentTestAgentModule>();
        });
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MicroAIOptions>(configuration.GetSection("AutogenConfig"));
        context.Services.Configure<TelegramTestOptions>(configuration.GetSection("NamingContest:Telegram"));
    }
}