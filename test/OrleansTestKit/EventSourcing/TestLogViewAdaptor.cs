using Orleans.EventSourcing;
using Orleans.Storage;

public class TestLogViewAdaptor<TView, TEntry> : ILogViewAdaptor<TView, TEntry>
    where TView : class, new()
    where TEntry : class
{
    private readonly ILogViewAdaptorHost<TView, TEntry> _hostGrain;
    private readonly TView _initialState;
    private readonly IGrainStorage _grainStorage;
    private readonly string _grainTypeName;
    private readonly ILogConsistencyProtocolServices _services;
    private readonly List<TEntry> _logEntries = new List<TEntry>();
    private TView _tentativeView;
    private TView _confirmedView;
    private int _confirmedVersion;

    public TestLogViewAdaptor(ILogViewAdaptorHost<TView, TEntry> hostGrain, TView initialState,
        IGrainStorage grainStorage, string grainTypeName, ILogConsistencyProtocolServices services)
    {
        _hostGrain = hostGrain;
        _initialState = initialState;
        _grainStorage = grainStorage;
        _grainTypeName = grainTypeName;
        _services = services;
        _tentativeView = initialState;
        _confirmedView = initialState;
        _confirmedVersion = 0;
    }

    public TView TentativeView => _tentativeView;
    public TView ConfirmedView => _confirmedView;
    public int ConfirmedVersion => _confirmedVersion;
    public IEnumerable<TEntry> UnconfirmedSuffix => _logEntries.Skip(_confirmedVersion);

    public void Submit(TEntry entry)
    {
        _logEntries.Add(entry);
    }

    public void SubmitRange(IEnumerable<TEntry> entries)
    {
        _logEntries.AddRange(entries);
    }

    public Task<bool> TryAppend(TEntry entry)
    {
        Submit(entry);
        return Task.FromResult(true);
    }

    public Task<bool> TryAppendRange(IEnumerable<TEntry> entries)
    {
        SubmitRange(entries);
        return Task.FromResult(true);
    }

    public Task ConfirmSubmittedEntries()
    {
        _confirmedVersion = _logEntries.Count;
        _confirmedView = _tentativeView;
        foreach (var logEntry in _logEntries)
        {
            PerformSubmit(DateTime.UtcNow, logEntry);
            //_hostGrain.UpdateView(_confirmedView, logEntry);
        }
        return Task.CompletedTask;
    }

    private const int Unconditional = -1;

    private void PerformSubmit(DateTime time, TEntry logEntry, int conditionalPosition = Unconditional, TaskCompletionSource<bool> resultPromise = null)
    {
        // var entry = new SubmissionEntry<TEntry>
        // {
        //     Entry = logEntry,
        //     SubmissionTime = time,
        //     ResultPromise = resultPromise,
        //     ConditionalPosition = conditionalPosition
        // };

        if (_tentativeView != null)
        {
            try
            {
                _hostGrain.UpdateView(_tentativeView, logEntry);
            }
            catch (Exception e)
            {
                _services.CaughtUserCodeException("UpdateView", nameof(PerformSubmit), e);
            }
        }

        try
        {
            _hostGrain.OnViewChanged(true, false);
        }
        catch (Exception e)
        {
            _services.CaughtUserCodeException("OnViewChanged", nameof(PerformSubmit), e);
        }
    }

    public Task Synchronize()
    {
        _tentativeView = _confirmedView;
        return Task.CompletedTask;
    }

    public void EnableStatsCollection() { }
    public void DisableStatsCollection() { }
    public LogConsistencyStatistics GetStats() => new LogConsistencyStatistics();

    public Task PreOnActivate() => Task.CompletedTask;
    public Task PostOnActivate() => Task.CompletedTask;
    public Task PostOnDeactivate() => Task.CompletedTask;

    public Task<IReadOnlyList<TEntry>> RetrieveLogSegment(int fromVersion, int toVersion)
    {
        var segment = _logEntries.Skip(fromVersion).Take(toVersion - fromVersion).ToList();
        return Task.FromResult<IReadOnlyList<TEntry>>(segment);
    }
}
