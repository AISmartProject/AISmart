namespace AISmart.GAgent.Core;

public static class AISmartGAgentConstants
{
    public const string EventHandlerDefaultMethodName = "HandleEventAsync";
    public const string SubscribersStateName = "Subscribers";
    public const string SubscriptionsStateName = "Subscriptions";
    public const string PublishersStateName = "Publishers";
    public const string ContextStorageGrainSelfTerminateReminderName = "DeleteSelfReminder";
    public static TimeSpan DefaultContextStorageGrainSelfDeleteTime = TimeSpan.FromMinutes(10);
}