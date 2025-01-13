using Neurointegration.Api.DataModels.Models;

namespace BotTemplate;

public static class MessageConstants
{
    public const string WithoutCoachButtonValue = "WithoutCoachButtonValue";
    public const string UnknownErrorText = "Что-то пошло не так! Попробуйте позже";
    public const string DefaultText = "Я пока не умею поддерживать стикеры, картинки и прочие нетекстовые сообщения!";
}

public static class CommandsConstants
{
    public const string Start = "/start"; 
    public const string ResultTables = "таблица результатов"; 
    public const string Settings = "настройки"; 
    public const string GetStudents = "мои ученики"; 
    public const string ChangeCoachStatus = "change_coach_status_flag"; 
    public const string ChangeSprintRegular = "change_sprints_regular_flag"; 
    public const string ChangeEveningStanUpTime = "change_evening_standup_time";
    public const string ChangeStatusTimeRange = "change_status_time_range";
    public const string ChangeRoutineActions = "change_routine_actions";
    public const string ReturnToRoutineActionsList = "return_routine_actions_list";
    public const string CancelEditRoutineActions = "cancel_edit_routine_actions";
    
    public const string StatusPanic = "Паника";
    public const string StatusOverexcitation = "Перевозбуждение";
    public const string StatusInclusion = "Включенность";
    public const string StatusBalance = "Баланс";
    public const string StatusRelaxation = "Расслабленность";
    public const string StatusPassivity = "Пассивность";
    public const string StatusApathy = "Апатия";

    public static string AddRoutineAction(RoutineType routineType) => $"/{routineType.ToString().ToLower()}Add";
    public static string DeleteRoutineAction(int i) => $"{DeleteRoutineActionPrefix}{i}";
    public const string DeleteRoutineActionPrefix = $"/actionDelete";

    public static string CheckupRoutineAction(string id) => $"/checkupAction{id}";
    public const string CheckupRoutineActionPrefix = $"/checkupAction";
    public const string StartCheckupRoutineActions = "start_checkup_routine";
    public const string FinishCheckupRoutineActions = "finish_checkup_routine";
    
    public const string FinishEveningStandUp = "finish_evening_stand_up";
}