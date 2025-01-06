namespace Neurointegration.Api.Settings;

public static class UserDbSettings
{
    public const string TableName =  "users_info";
    
    public const string UserIdField = "chat_id";
    public const string EveningStandUpTimeField = "evening_standup_time";
    public const string WeekReflectionTime = "week_reflection_time";
    public const string MessageStartTimeField = "start_message_time";
    public const string MessageEndTimeField = "end_message_time";
    public const string RoutineActionsField = "routine_actions";

    public const string EmailField = "email";
    public const string IAmCoachField = "iam_coach";
    public const string SendRegularMessagesField = "send_regular_messages";
    public const string UsernameField = "username";
}