namespace AISmart.GAgent.Core;

public static class AISmartGAgentConstants
{
    public const string EventHandlerDefaultMethodName = "HandleEventAsync";
    public const string SubscribersStateName = "Subscribers";
    public const string SubscriptionsStateName = "Subscriptions";
    public const string PublishersStateName = "Publishers";
    public static TimeSpan StateSaveTimerDueTime = TimeSpan.FromMilliseconds(10);
    public static TimeSpan StateSaveTimerPeriod = TimeSpan.FromMilliseconds(10);
}