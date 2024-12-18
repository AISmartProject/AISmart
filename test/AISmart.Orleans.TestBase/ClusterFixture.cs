using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Application.Grains;
using AISmart.Mock;
using AISmart.Provider;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Hosting;
using Orleans.TestingHost;
using Volo.Abp.AutoMapper;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Reflection;

public class ClusterFixture : IDisposable, ISingletonDependency
{
    public static MockLoggerProvider LoggerProvider { get; set; }

    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        builder.AddClientBuilderConfigurator<TestClientBuilderConfigurator>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose()
    {
        Cluster.StopAllSilos();
    }

    public TestCluster Cluster { get; private set; }

    private class TestSiloConfigurations : ISiloConfigurator
    {
        public void Configure(ISiloBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                services.AddAutoMapper(typeof(AIApplicationGrainsModule).Assembly);
                var mock = new Mock<ILocalEventBus>();
                services.AddSingleton(typeof(ILocalEventBus), mock.Object);

                // Configure logging
                var loggerProvider = new MockLoggerProvider("AISmart");
                services.AddSingleton<ILoggerProvider>(loggerProvider);
                LoggerProvider = loggerProvider;
                services.AddLogging(logging =>
                {
                    //logging.AddProvider(loggerProvider);
                    logging.AddConsole(); // Adds console logger
                });
                var aelfNodeProvider = new MockAElfNodeProvider();
                services.AddSingleton<IAElfNodeProvider>(aelfNodeProvider);
                services.OnExposing(onServiceExposingContext =>
                {
                    var implementedTypes = ReflectionHelper.GetImplementedGenericTypes(
                        onServiceExposingContext.ImplementationType,
                        typeof(IObjectMapper<,>)
                    );
                });

                services.AddTransient(typeof(IObjectMapper<>), typeof(DefaultObjectMapper<>));
                services.AddTransient(typeof(IObjectMapper), typeof(DefaultObjectMapper));
                services.AddTransient(typeof(IAutoObjectMappingProvider), typeof(AutoMapperAutoObjectMappingProvider));
                services.AddTransient(sp => new MapperAccessor()
                {
                    Mapper = sp.GetRequiredService<IMapper>()
                });
                services.AddTransient<IMapperAccessor>(provider => provider.GetRequiredService<MapperAccessor>());
            })
            .AddMemoryStreams("AISmart")
            .AddMemoryGrainStorage("PubSubStore")
            //.AddMemoryGrainStorageAsDefault()
            .AddLogStorageBasedLogConsistencyProvider("LogStorage");
        }
    }

    public class MapperAccessor : IMapperAccessor
    {
        public IMapper Mapper { get; set; }
    }

    private class TestClientBuilderConfigurator : IClientBuilderConfigurator
    {
        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder) => clientBuilder
            .AddMemoryStreams("AISmart");
    }
    
    public static async Task WaitLogAsync(string log)
    {
        var timeout = TimeSpan.FromSeconds(15);
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < timeout)
        {
            if (LoggerProvider.Logs.Any(l => l.Contains(log)))
            {
                break;
            }
            await Task.Delay(1000);
        }
    }
}