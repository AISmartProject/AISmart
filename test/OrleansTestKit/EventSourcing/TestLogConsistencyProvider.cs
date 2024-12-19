using AISmart.EventSourcing.Core.Storage;
using Orleans.EventSourcing;
using Orleans.Storage;

namespace Orleans.TestKit;

public class TestLogConsistencyProvider : ILogViewAdaptorFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogConsistentStorage _logConsistentStorage;

    public TestLogConsistencyProvider(IServiceProvider serviceProvider, ILogConsistentStorage logConsistentStorage)
    {
        _serviceProvider = serviceProvider;
        _logConsistentStorage = logConsistentStorage;
    }

    public ILogViewAdaptor<TLogView, TLogEntry> MakeLogViewAdaptor<TLogView, TLogEntry>(
        ILogViewAdaptorHost<TLogView, TLogEntry> hostGrain, TLogView initialState,
        string grainTypeName, IGrainStorage grainStorage, ILogConsistencyProtocolServices services)
        where TLogView : class, new() where TLogEntry : class
    {
        return new LogViewAdaptor<TLogView, TLogEntry>(hostGrain, initialState, grainStorage, grainTypeName,
            new TestLogConsistencyProtocolServices(), _logConsistentStorage, null);
    }

    public bool UsesStorageProvider => true;
}
