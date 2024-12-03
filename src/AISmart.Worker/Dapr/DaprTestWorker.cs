using AISmart.Authors;
using AISmart.Dapr;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace AISmart.Worker.Dapr;
// Test dapr demo, will be deleted in few days
public class DaprTestWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IDaprProvider _daprProvider;
    public DaprTestWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IDaprProvider daprProvider
    ) : base(
        timer, 
        serviceScopeFactory)
    {
        _daprProvider = daprProvider;
        Timer.Period = 10000; //10 seconds
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogInformation("Starting dapr...");
        await  _daprProvider.PublishEventAsync(DaprConstants.PubSubName, DaprConstants.TestGroup, new AuthorDto
        {
            Id = default,
            Name = "Test",
            ShortBio = "123"
        });
    }
}