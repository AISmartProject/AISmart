using AISmart.CQRS.Handler;
using AISmart.CQRS.Provider;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AISmart.CQRS;

public class AISmartCQRSModule : AbpModule
{
       public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options => { options.AddMaps<AISmartCQRSModule>(); });

            context.Services.AddMediatR(typeof(SaveStateCommandHandler).Assembly);
            context.Services.AddMediatR(typeof(GetStateQueryHandler).Assembly);
            context.Services.AddMediatR(typeof(SendEventCommandHandler).Assembly);
            context.Services.AddSingleton<IIndexingService, ElasticIndexingService>();
            context.Services.AddSingleton<ICQRSProvider,CQRSProvider>();
            context.Services.AddTransient<SaveStateCommandHandler>();
            context.Services.AddTransient<GetStateQueryHandler>();
            context.Services.AddTransient<SendEventCommandHandler>();

        }
        
}