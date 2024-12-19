using AISmart.GAgent.Autogen.Options;
using AISmart.Options;
using AISmart.Provider;
using AISmart.Rag;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AISmart.GAgent.Autogen;

[DependsOn(
    typeof(AISmartApplicationContractsModule),
    typeof(AISmartRagModule)
)]
public class AISmartGAgentAutogenModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<AutogenOptions>(configuration.GetSection("AutogenConfig"));
        //
        // var autogenConfig = context.Services.GetRequiredService<IOptions<AutogenOptions>>().Value;
        // context.Services.AddTransient<ChatClient>(op => new ChatClient(autogenConfig.Model, autogenConfig.ApiKey));
        context.Services.AddTransient<IChatAgentProvider, ChatAgentProvider>();
        // context.Services.AddTransient<IChatService, ChatService>();
        Configure<RagOptions>(configuration.GetSection("AutogenConfig:AutoGenRag"));
    }
}