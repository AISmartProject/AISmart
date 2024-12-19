using AISmart.Agents;
using AISmart.EventSourcing.Core;
using AISmart.EventSourcing.Core.Storage;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.EventSourcing;
using Orleans.Serialization;
using Orleans.Serialization.Cloning;
using Orleans.Serialization.Configuration;
using Orleans.Serialization.Serializers;
using Orleans.Storage;
using Orleans.TestKit.Storage;

namespace Orleans.TestKit;

public class TestLogConsistencyProvider : ILogViewAdaptorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public TestLogConsistencyProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ILogViewAdaptor<TLogView, TLogEntry> MakeLogViewAdaptor<TLogView, TLogEntry>(
        ILogViewAdaptorHost<TLogView, TLogEntry> hostGrain, TLogView initialState,
        string grainTypeName, IGrainStorage grainStorage, ILogConsistencyProtocolServices services)
        where TLogView : class, new() where TLogEntry : class
    {
        var mockOptionsManager = new Mock<IOptions<TypeManifestOptions>>();
        mockOptionsManager.Setup(m => m.Value).Returns(new TypeManifestOptions());
        var codecProvider = new CodecProvider(_serviceProvider, mockOptionsManager.Object);
        return new LogViewAdaptor<TLogView, TLogEntry>(hostGrain, initialState, grainStorage, grainTypeName,
            new TestLogConsistencyProtocolServices(), new InMemoryLogConsistentStorage(),
            new DeepCopier(codecProvider, new CopyContextPool(codecProvider)));
    }

    public bool UsesStorageProvider => true;
}
