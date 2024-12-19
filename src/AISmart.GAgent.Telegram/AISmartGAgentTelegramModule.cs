using AISmart.Options;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AISmart;

[DependsOn(
    typeof(AISmartApplicationContractsModule)
    )]
public class AISmartGAgentTelegramModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AISmartGAgentTelegramModule>();
        });
        var configuration = context.Services.GetConfiguration();
        Configure<TelegramOptions>(configuration.GetSection("Telegram")); 
    }
}
