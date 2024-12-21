using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;
using Volo.Abp.Modularity;

namespace AISmart.AutoGen.Tests;

[DependsOn(
    typeof(AISmartTestBaseModule)
)]
public class AISmartAutoGenTestModule: AbpModule
{   
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<ChatClient>( sp =>
        {
            IConfiguration configuration = context.Services.GetRequiredService<IConfiguration>();
            var apiKey = configuration.GetSection("Chat:APIKey").Value;
            var modelId = configuration.GetSection("Chat:Model").Value;
            return new OpenAIClient(apiKey).GetChatClient(modelId);
        });
    }
}