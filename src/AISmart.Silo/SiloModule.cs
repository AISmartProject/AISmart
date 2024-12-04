using AElf.OpenTelemetry;
using AISmart.Domain.Grains;
using Microsoft.Extensions.DependencyInjection;
using AISmart.Application.Grains;
using Nest;
using Serilog;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
namespace AISmart.Silo;

[DependsOn(
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAutofacModule),
    typeof(OpenTelemetryModule)
)]
public class SiloModule : AbpModule, IDomainGrainsModule, IApplicationGrainsModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<SiloModule>(); });
        context.Services.AddHostedService<AISmartHostedService>();
        var configuration = context.Services.GetConfiguration();
        // add elasticSearch
        context = AddElasticSearch(context);
        
        context.Services.AddSerilog(loggerConfiguration => {},
            true, writeToProviders: true);
        
        
        context.Services.AddHttpClient();
    }

    private ServiceConfigurationContext AddElasticSearch(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.AddSingleton<IElasticClient>(provider =>
        {
            var settings =new ConnectionSettings(new Uri(configuration["Elasticsearch:Uri"]))
                .DefaultIndex("grain-events");
            return new ElasticClient(settings);
        });

        return context;
    }
}