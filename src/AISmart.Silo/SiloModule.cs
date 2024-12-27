using AElf.OpenTelemetry;
using AISmart.Domain.Grains;
using Microsoft.Extensions.DependencyInjection;
using AISmart.Application.Grains;
using AISmart.GAgent.Autogen;
using AiSmart.GAgent.SocialAgent;
using AiSmart.GAgent.TestAgent;
using Serilog;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
namespace AISmart.Silo;

[DependsOn(
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAutofacModule),
    typeof(OpenTelemetryModule),
    typeof(AISmartGAgentAElfModule),
    typeof(AISmartGAgentPumpfunModule),
    typeof(AISmartGAgentTelegramModule),
    typeof(AISmartGAgentTwitterModule),
    typeof(AISmartGAgentAutogenModule),
    typeof(AISmartGAgentMicroAIModule),
    typeof(AISmartGAgentTestAgentModule),
    typeof(AISmartGAgentSocialGAgentModule)
)]
public class SiloModule : AIApplicationGrainsModule, IDomainGrainsModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<SiloModule>(); });
        context.Services.AddHostedService<AISmartHostedService>();
        var configuration = context.Services.GetConfiguration();
        //add dependencies here
        context.Services.AddSerilog(loggerConfiguration => {},
            true, writeToProviders: true);
        context.Services.AddHttpClient();
    }
}