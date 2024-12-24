using System;
using System.Linq;
using System.Threading.Tasks;
using AISmart.CQRS.Handler;
using AISmart.CQRS.Options;
using AISmart.CQRS.Provider;
using AISmart.GAgent.Core;
using AISmart.CQRS.Service;
using Elasticsearch.Net;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using Volo.Abp;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AISmart.CQRS;

public class AISmartCQRSModule : AbpModule
{
       public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options => { options.AddMaps<AISmartCQRSModule>(); });
            var configuration = context.Services.GetConfiguration();
            Configure<KafkaOptions>(configuration.GetSection("Kafka"));

            context.Services.AddTransient<KafkaProducerService>();
            context.Services.AddTransient<KafkaConsumerService>();
            
            context.Services.AddMediatR(typeof(SaveStateCommandHandler).Assembly);
            context.Services.AddMediatR(typeof(GetStateQueryHandler).Assembly);
            context.Services.AddMediatR(typeof(SendEventCommandHandler).Assembly);
            context.Services.AddSingleton<IIndexingService, ElasticIndexingService>();
            context.Services.AddSingleton<IEventDispatcher, CQRSProvider>();
            context.Services.AddSingleton<ICQRSProvider, CQRSProvider>();
            context.Services.AddTransient<SaveStateCommandHandler>();
            context.Services.AddTransient<GetStateQueryHandler>();
            context.Services.AddTransient<SendEventCommandHandler>();
            ConfigureElasticsearch(context, configuration);
            
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
       
       public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
       {
           var appLifetime = context.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
           var consumerService = context.ServiceProvider.GetRequiredService<KafkaConsumerService>();
           var cancellationToken = appLifetime.ApplicationStopping;
           await Task.Run(() => consumerService.StartConsuming(cancellationToken), cancellationToken);
       }
}