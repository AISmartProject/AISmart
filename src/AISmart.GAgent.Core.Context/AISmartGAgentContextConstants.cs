namespace AISmart.GAgent.Core.Context;

public class AISmartGAgentContextConstants
{
    public const string ContextStorageGrainSelfTerminateReminderName = "DeleteSelfReminder";
    public static TimeSpan DefaultContextStorageGrainSelfDeleteTime = TimeSpan.FromMinutes(10);
}