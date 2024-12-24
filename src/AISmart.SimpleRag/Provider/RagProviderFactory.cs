using System.Collections.Concurrent;
using AISmart.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace AISmart.Provider;

public class RagProviderFactory : IRagProviderFactory, ISingletonDependency
{
    private readonly IOptionsMonitor<RagOptions> _ragOptionsMonitor;
    private readonly ConcurrentDictionary<string, RagProvider> _providersDict;
    private readonly ILogger<RagProvider> _logger;

    public RagProviderFactory(IOptionsMonitor<RagOptions> ragOptionsMonitor,  ILogger<RagProvider> logger)
    {
        _ragOptionsMonitor = ragOptionsMonitor;
        _providersDict = new ConcurrentDictionary<string, RagProvider>();
        _logger = logger;
    }

    public RagProvider GetProvider(string configName)
    {
        var options = _ragOptionsMonitor.Get(configName);
        if (_providersDict.TryGetValue(configName, out var provider))
        {
            return provider;
        }

        provider = new RagProvider(Microsoft.Extensions.Options.Options.Create(options), _logger);
        _providersDict[configName] = provider;
        return provider;
    }
}

public interface IRagProviderFactory
{
    RagProvider GetProvider(string configName);
}