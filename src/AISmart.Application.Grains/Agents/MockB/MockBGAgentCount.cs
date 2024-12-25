using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace AISmart.Application.Grains.Agents.MockB;

public interface IMockBGAgentCount
{
    Task BGAgentCount();
}

public class MockBGAgentCount : ISingletonDependency, IMockBGAgentCount
{
    private int _bGAgentCount;
    private readonly ILogger<MockBGAgentCount> _logger;

    public MockBGAgentCount(ILogger<MockBGAgentCount> logger)
    {
        _logger = logger;
    }


    public async Task BGAgentCount()
    {
        _bGAgentCount++;

        _logger.LogInformation($"MockBGAgentCount: {_bGAgentCount}");
    }
}