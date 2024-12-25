using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace AISmart.Application.Grains.Agents.MockA;

public interface IMockAGAgentCount
{
    Task AGAgentCount();
}

public class MockAGAgentCount : ISingletonDependency, IMockAGAgentCount
{
    private int _aGAgentCount;
    private readonly ILogger<MockAGAgentCount> _logger;

    public MockAGAgentCount(ILogger<MockAGAgentCount> logger)
    {
        _logger = logger;
    }


    public async Task AGAgentCount()
    {
        _aGAgentCount++;

        _logger.LogInformation($"MockAGAgentCount: {_aGAgentCount}");
    }
}