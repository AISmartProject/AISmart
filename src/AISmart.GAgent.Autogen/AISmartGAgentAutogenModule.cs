using AISmart.GAgent.Autogen.Options;
using AISmart.Rag;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AISmart.GAgent.Autogen;

[DependsOn(
    typeof(AISmartApplicationContractsModule)
)]
public class AISmartGAgentAutogenModule: AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<AutogenOptions>(configuration.GetSection("AutogenConfig"));

        var autogenConfig = context.Services.GetRequiredService<IOptions<AutogenOptions>>().Value;
        context.Services.AddTransient<ChatClient>(op => new ChatClient(autogenConfig.Model, autogenConfig.ApiKey));
    }
}