using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AISmart.CQRS;

public class AISmartCQRSModule : AbpModule
{
       public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options => { options.AddMaps<AISmartCQRSModule>(); });

            context.Services.AddSingleton<IIndexingService, ElasticIndexingService>();
           
        }
        
}