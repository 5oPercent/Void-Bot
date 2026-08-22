namespace Void.Bot.Message.Constants.EventAction;

public static class EventActionFailurePolicies
{
    public const string StopAll = "StopAll";

    public const string StopRule = "StopRule";

    public const string Continue = "Continue";

    public static readonly string[] All = [StopAll, StopRule, Continue];
}

public static class EventActionConditionOperators
{
    public const string Equal = "Equal";

    public const string NotEqual = "NotEqual";

    public const string In = "In";

    public const string NotIn = "NotIn";
}

public static class EventActionExecutionStatuses
{
    public const string Succeeded = "Succeeded";

    public const string Failed = "Failed";
}
