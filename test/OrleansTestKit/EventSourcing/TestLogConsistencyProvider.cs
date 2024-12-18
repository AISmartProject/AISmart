using Orleans.EventSourcing;
using Orleans.Storage;

namespace Orleans.TestKit;

public class TestLogConsistencyProvider : ILogViewAdaptorFactory
{
    public ILogViewAdaptor<TLogView, TLogEntry> MakeLogViewAdaptor<TLogView, TLogEntry>(
        ILogViewAdaptorHost<TLogView, TLogEntry> hostGrain, TLogView initialState,
        string grainTypeName, IGrainStorage grainStorage, ILogConsistencyProtocolServices services)
        where TLogView : class, new() where TLogEntry : class =>
        new TestLogViewAdaptor<TLogView, TLogEntry>(hostGrain, initialState, grainStorage, grainTypeName, services);

    public bool UsesStorageProvider => true;
}
