using System;
using System.Linq;
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

            context.Services.AddMediatR(typeof(SaveStateCommandHandler).Assembly);
            context.Services.AddMediatR(typeof(GetStateQueryHandler).Assembly);
            context.Services.AddMediatR(typeof(SendEventCommandHandler).Assembly);
            context.Services.AddMediatR(typeof(SaveGEventCommandHandler).Assembly);

            context.Services.AddSingleton<IIndexingService, ElasticIndexingService>();
            context.Services.AddSingleton<IEventDispatcher, CQRSProvider>();
            context.Services.AddSingleton<ICQRSProvider, CQRSProvider>();
            context.Services.AddTransient<SaveStateCommandHandler>();
            context.Services.AddTransient<GetStateQueryHandler>();
            context.Services.AddTransient<SendEventCommandHandler>();
            context.Services.AddTransient<SaveGEventCommandHandler>();
            var configuration = context.Services.GetConfiguration();
            //ConfigureElasticsearch(context, configuration);

        }
       private static void ConfigureElasticsearch(
           ServiceConfigurationContext context,
           IConfiguration configuration)
       {
           context.Services.AddSingleton<IElasticClient>(sp =>
           {
               var uris = configuration.GetSection("ElasticUris:Uris").Get<string[]>();
               if (uris == null || uris.Length == 0)
               {
                   throw new ArgumentNullException("ElasticUris:Uris", "Elasticsearch URIs cannot be null or empty.");
               }

               var settings = new ConnectionSettings(new StaticConnectionPool(uris.Select(uri => new Uri(uri)).ToArray()));

               return new ElasticClient(settings);
           });
    
       } 
}