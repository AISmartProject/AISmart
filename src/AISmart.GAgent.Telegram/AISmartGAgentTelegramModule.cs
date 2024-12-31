using AISmart.GAgent.Telegram.Options;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace AISmart.GAgent.Telegram;

[DependsOn(
    typeof(AISmartApplicationContractsModule)
    )]
// ReSharper disable once InconsistentNaming
public class AISmartGAgentTelegramModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        Configure<TelegramOptions>(configuration.GetSection("Telegram")); 
    }
}
