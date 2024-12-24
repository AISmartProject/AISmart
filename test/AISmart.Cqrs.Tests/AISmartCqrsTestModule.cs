using AISmart.CQRS.Options;
using AISmart.Options;
using AISmart.Service;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace AISmart.Cqrs.Tests;
[DependsOn(
    typeof(AISmartApplicationModule),
    typeof(AbpEventBusModule),
    typeof(AISmartOrleansTestBaseModule),
    typeof(AISmartDomainTestModule),
    typeof(AISmartApplicationTestModule)
)]
public class AISmartCqrsTestModule: AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        base.ConfigureServices(context);
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<AISmartCqrsTestModule>(); });
        var configuration = context.Services.GetConfiguration();
        Configure<ChatConfigOptions>(configuration.GetSection("Chat"));   
        context.Services.AddSingleton<ICqrsService, CqrsService>();
        Configure<KafkaOptions>(configuration.GetSection("Kafka"));

    }
}