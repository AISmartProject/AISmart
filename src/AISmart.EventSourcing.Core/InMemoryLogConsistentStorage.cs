using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using AISmart.EventSourcing.Core.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

namespace AISmart.EventSourcing.Core;

public class LogEntry
{
    public int Version { get; set; }
    public string Data { get; set; } // Adjust the type as needed
}

public class InMemoryLogConsistentStorage : ILogConsistentStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly string _name = "AISmart";
    private readonly string _serviceId = "AISmart";
    private readonly ConcurrentDictionary<string, List<LogEntry>> _storage = new();

    private bool _initialized;

    public Task<IReadOnlyList<TLogEntry>> ReadAsync<TLogEntry>(string grainTypeName, GrainId grainId,
        int fromVersion, int maxCount)
    {
        if (!_initialized || maxCount <= 0)
        {
            return Task.FromResult<IReadOnlyList<TLogEntry>>(new List<TLogEntry>());
        }

        var collectionName = GetStreamName(grainId);
        if (!_storage.TryGetValue(collectionName, out var entries))
        {
            return Task.FromResult<IReadOnlyList<TLogEntry>>(new List<TLogEntry>());
        }

        var results = entries
            .Where(entry => entry.Version >= fromVersion)
            .OrderBy<LogEntry, object>(entry => entry.Version)
            .Take(maxCount)
            .Select(entry => JsonSerializer.Deserialize<TLogEntry>(entry.Data))
            .ToList();

        return Task.FromResult<IReadOnlyList<TLogEntry>>(results);
    }

    public Task<int> GetLastVersionAsync(string grainTypeName, GrainId grainId)
    {
        if (!_initialized)
        {
            return Task.FromResult(-1);
        }

        var collectionName = GetStreamName(grainId);
        if (!_storage.TryGetValue(collectionName, out var entries) || !entries.Any())
        {
            return Task.FromResult(-1);
        }

        var lastVersion = entries.Max(entry => entry.Version);
        return Task.FromResult(lastVersion);
    }

    public async Task<int> AppendAsync<TLogEntry>(string grainTypeName, GrainId grainId, IList<TLogEntry> entries,
        int expectedVersion)
    {
        if (!_initialized)
        {
            return -1;
        }

        var collectionName = GetStreamName(grainId);
        if (entries.Count == 0)
        {
            return await GetLastVersionAsync(grainTypeName, grainId);
        }

        if (!_storage.TryGetValue(collectionName, out var logEntries))
        {
            logEntries = new List<LogEntry>();
            _storage[collectionName] = logEntries;
        }

        var currentVersion = await GetLastVersionAsync(grainTypeName, grainId);
        if (currentVersion != expectedVersion)
        {
            throw new InconsistentStateException(
                $"Version conflict ({nameof(AppendAsync)}): ServiceId={_serviceId} ProviderName={_name} GrainType={grainTypeName} GrainId={grainId} Version={expectedVersion}.");
        }

        foreach (var entry in entries)
        {
            var logEntry = new LogEntry
            {
                Version = ++currentVersion,
                Data = JsonSerializer.Serialize(entry)
            };
            logEntries.Add(logEntry);
        }

        return currentVersion;
    }

    public void Participate(ISiloLifecycle observer)
    {
        var name = OptionFormattingUtilities.Name<InMemoryLogConsistentStorage>(_name);
        observer.Subscribe(name, ServiceLifecycleStage.ApplicationServices, Init, Close);
    }

    private Task Init(CancellationToken cancellationToken)
    {
        _initialized = true;
        return Task.CompletedTask;
    }

    private Task Close(CancellationToken cancellationToken)
    {
        if (!_initialized)
        {
            return Task.CompletedTask;
        }

        try
        {
            _storage.Clear();
        }
        catch (Exception ex)
        {
            throw new Exception(FormattableString.Invariant($"{ex.GetType()}: {ex.Message}"));
        }

        return Task.CompletedTask;
    }

    private string GetStreamName(GrainId grainId)
    {
        return $"{_serviceId}/{_name}/log/{grainId}";
    }
}