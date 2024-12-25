using Microsoft.Extensions.Logging;

namespace AISmart.GAgent.Core.Context;

public sealed class ContextStorageState
{
    public Dictionary<string, object?> Context { get; set; } = new();
}

public interface IContextStorageGrain : IGrainWithGuidKey
{
    Task AddContext(string key, object? value);
    Task AddContext(Dictionary<string, object?> context);
    Task<Dictionary<string, object?>> GetContext();
    Task ResetSelfTerminateTime(TimeSpan timeSpan);
}

public class ContextStorageGrain : Grain<ContextStorageState>, IContextStorageGrain, IRemindable
{
    private readonly IPersistentState<ContextStorageState> _context;
    private readonly ILogger<ContextStorageGrain> _logger;
    private IGrainReminder _reminder;

    public ContextStorageGrain([PersistentState("ContextStorage")] IPersistentState<ContextStorageState> context,
        ILogger<ContextStorageGrain> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task AddContext(string key, object? value)
    {
        _logger.LogInformation($"Adding context {key}: {value}, ContextStorageGrain's GrainId: {this.GetGrainId()}");

        _context.ReadStateAsync();

        if (_context.State.Context.IsNullOrEmpty())
        {
            _context.State.Context = [];
        }

        _context.State.Context[key] = value;
        _context.WriteStateAsync();
        return Task.CompletedTask;
    }

    public Task AddContext(Dictionary<string, object?> context)
    {
        _context.ReadStateAsync();

        if (_context.State.Context.IsNullOrEmpty())
        {
            _context.State.Context = context;
        }
        else
        {
            foreach (var keyPair in context)
            {
                _logger.LogInformation(
                    $"Adding context {keyPair.Key}: {keyPair.Value}, ContextStorageGrain's GrainId: {this.GetGrainId()}");
                _context.State.Context[keyPair.Key] = keyPair.Value;
            }
        }

        _context.WriteStateAsync();
        return Task.CompletedTask;
    }

    public Task<Dictionary<string, object?>> GetContext()
    {
        _context.ReadStateAsync();
        return Task.FromResult(_context.State.Context);
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName == "DeleteSelfReminder")
        {
            _logger.LogInformation($"Reminder triggered for grain {this.GetGrainId()}, deleting grain.");
            await ClearStateAsync();
            DeactivateOnIdle();
        }
    }

    public async Task ResetSelfTerminateTime(TimeSpan timeSpan)
    {
        _reminder = await this.RegisterOrUpdateReminder(
            AISmartGAgentContextConstants.ContextStorageGrainSelfTerminateReminderName,
            timeSpan, timeSpan);
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _reminder = await this.RegisterOrUpdateReminder(
            AISmartGAgentContextConstants.ContextStorageGrainSelfTerminateReminderName,
            AISmartGAgentContextConstants.DefaultContextStorageGrainSelfDeleteTime,
            AISmartGAgentContextConstants.DefaultContextStorageGrainSelfDeleteTime);
        await base.OnActivateAsync(cancellationToken);
    }
}