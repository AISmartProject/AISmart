using AISmart.AgentsNetwork;
using System;
using AISmart.CQRS.Handler;
using System;
using AISmart.AgentsNetwork;
using AISmart.CQRS.Handler;
using AISmart.CQRS.Options;
using AISmart.Options;
using MediatR;
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
    typeof(AISmartDomainTestModule)
)]
public class AISmartApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        base.ConfigureServices(context);
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<AISmartApplicationModule>(); });
        var configuration = context.Services.GetConfiguration();
        Configure<ChatConfigOptions>(configuration.GetSection("Chat"));   
        Configure<AgentNetworkConfigOptions>(configuration.GetSection("NetWork")); 
        Configure<RagOptions>(configuration.GetSection("Rag"));   
        context.Services.AddMediatR(typeof(GetStateQueryHandler).Assembly);
        Configure<KafkaOptions>(configuration.GetSection("Kafka"));

    }
}