using Orleans.Storage;
using Orleans.TestKit.Storage;

namespace Orleans.TestKit;

public class TestGrainStorage : IGrainStorage
{
    private readonly StorageManager _storageManager;

    public TestGrainStorage(StorageManager storageManager)
    {
        _storageManager = storageManager;
    }

    public Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var storage = _storageManager.GetStorage<T>(stateName);
        grainState.State = storage.State;
        return Task.CompletedTask;
    }

    public Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var storage = _storageManager.GetStorage<T>(stateName);
        storage.State = grainState.State;
        return Task.CompletedTask;
    }

    public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        return Task.CompletedTask;
    }
}