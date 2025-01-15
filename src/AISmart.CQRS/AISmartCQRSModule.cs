using System;
using System.Linq;
using AISmart.CQRS.Dto;
using AISmart.CQRS.Handler;
using AISmart.CQRS.Provider;
using AISmart.GAgent.Core;
using Elasticsearch.Net;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AISmart.CQRS;

public class AISmartCQRSModule : AbpModule
{
       public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options => { options.AddMaps<AISmartCQRSModule>(); });
            context.Services.AddAutoMapper(typeof(AISmartCQRSAutoMapperProfile).Assembly);

            context.Services.AddMediatR(typeof(SaveStateCommandHandler).Assembly);
            context.Services.AddMediatR(typeof(GetStateQueryHandler).Assembly);
            context.Services.AddMediatR(typeof(SendEventCommandHandler).Assembly);
            context.Services.AddMediatR(typeof(SaveGEventCommandHandler).Assembly);
            context.Services.AddMediatR(typeof(GetGEventQueryHandler).Assembly);
            context.Services.AddMediatR(typeof(SaveLogCommandHandler).Assembly);
            context.Services.AddMediatR(typeof(GetLogQueryHandler).Assembly);

            context.Services.AddSingleton<IIndexingService, ElasticIndexingService>();
            context.Services.AddSingleton<IEventDispatcher, CQRSProvider>();
            context.Services.AddSingleton<ICQRSProvider, CQRSProvider>();
            context.Services.AddTransient<SaveStateCommandHandler>();
            context.Services.AddTransient<GetStateQueryHandler>();
            context.Services.AddTransient<SendEventCommandHandler>();
            context.Services.AddTransient<SaveGEventCommandHandler>();
            context.Services.AddTransient<GetGEventQueryHandler>();
            context.Services.AddTransient<SaveLogCommandHandler>();
            context.Services.AddTransient<GetLogQueryHandler>();

            var configuration = context.Services.GetConfiguration();
            ConfigureElasticsearch(context, configuration);

        }
       private static void ConfigureElasticsearch(
           ServiceConfigurationContext context,
           IConfiguration configuration)
       {
           context.Services.AddSingleton<IElasticClient>(provider =>
           {
               var settings =new ConnectionSettings(new Uri("http://127.0.0.1:9200"))
                   .DisablePing()
                   .DisableDirectStreaming()
                   .DefaultIndex("cqrs").DefaultFieldNameInferrer(fieldName => 
                       char.ToLowerInvariant(fieldName[0]) + fieldName[1..]);
               return new ElasticClient(settings);
           });
    
       } 
}