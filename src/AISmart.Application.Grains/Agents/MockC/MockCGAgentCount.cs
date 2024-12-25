using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace AISmart.Application.Grains.Agents.MockC;

public interface IMockCGAgentCount
{
    Task CGAgentCount();
}

public class MockCGAgentCount : ISingletonDependency, IMockCGAgentCount
{
    private int _cGAgentCount;
    private readonly ILogger<MockCGAgentCount> _logger;

    public MockCGAgentCount(ILogger<MockCGAgentCount> logger)
    {
        _logger = logger;
    }


    public async Task CGAgentCount()
    {
        _cGAgentCount++;

        _logger.LogInformation($"MockCGAgentCount: {_cGAgentCount}");
    }
}