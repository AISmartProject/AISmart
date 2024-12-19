using AISmart.EventSourcing.Core.LogConsistency;
using AISmart.EventSourcing.Core.Storage;
using AISmart.EventSourcing.MongoDB.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Orleans.Configuration;
using Orleans.EventSourcing;

namespace AISmart.EventSourcing.MongoDB;

public static class MongoDbStorageSiloBuilderExtensions
{
    public static ISiloBuilder AddMongoDbStorageBasedLogConsistencyProvider(this ISiloBuilder builder, string name = "LogStorage")
    {
        builder.ConfigureServices(services =>
        {
            services.AddMongoDbStorageBasedLogConsistencyProvider(name);
        });

        return builder;
    }
    
    internal static void AddMongoDbStorageBasedLogConsistencyProvider(this IServiceCollection services, string name)
    {
        services.AddSingleton<ILogConsistencyProtocolServices, DefaultProtocolServices>();
        services.AddSingleton<MongoDbStorageOptions>();
        services.AddOptions<MongoDbStorageOptions>()
            .Configure(options =>
            {
                var configuration = services.GetConfiguration().GetSection("Orleans");
                options.ClientSettings =
                    MongoClientSettings.FromConnectionString(configuration.GetValue<string>("MongoDBClient"));
            });
        services.AddKeyedSingleton<ILogConsistentStorage>(name, MongoDbLogConsistentStorageFactory.Create);
        services.ConfigureNamedOptionForLogging<MongoDbStorageOptions>(name)
            .TryAddSingleton(sp => sp.GetKeyedService<ILogViewAdaptorFactory>("Default"));
    }
}