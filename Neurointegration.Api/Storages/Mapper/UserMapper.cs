using System.Net.Http.Headers;
using System.Text;
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.Settings;
using Ydb.Sdk.Value;

namespace Neurointegration.Api.Storages.Mapper;

public class UserMapper
{
    public DataModels.Models.User ToUserEntity(ResultSet.Row row)
    {
        return new DataModels.Models.User()
        {
            UserId = row[UserDbSettings.UserIdField].GetInt64(),
            Email = row[UserDbSettings.EmailField].GetUtf8(),
            Username = row[UserDbSettings.UsernameField].GetUtf8(),
            IAmCoach = row[UserDbSettings.IAmCoachField].GetBool(),
            SendRegularMessages = row[UserDbSettings.SendRegularMessagesField].GetBool(),

            EveningStandUpTime = row[UserDbSettings.EveningStandUpTimeField].GetOptionalInterval(),
            WeekReflectionTime = row[UserDbSettings.WeekReflectionTime].GetOptionalInterval(),
            MessageStartTime = row[UserDbSettings.MessageStartTimeField].GetOptionalInterval(),
            MessageEndTime = row[UserDbSettings.MessageEndTimeField].GetOptionalInterval(),
            RoutineActions = ToRoutineActionsList(row[UserDbSettings.RoutineActionsField].GetUtf8())
        };
    }

    public List<RoutineAction> ToRoutineActionsList(string text)
    {
        var actionLines = text.Split(";");
        var result = new List<RoutineAction>();
        foreach (var line in actionLines)
        {
            var parsedAction = line.Split("::");
            var type = Enum.Parse<RoutineType>(parsedAction[0]);
            var action = parsedAction[1];
            result.Add(
                new RoutineAction()
                {
                    Type = type,
                    Action = action
                });
        }

        return result;
    }

    public string ToString(List<RoutineAction> routineActions)
    {
        var sb = new StringBuilder();
        foreach (var routine in routineActions)
        {
            sb.Append($"{routine.Type}::{routine.Action};");
        }

        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }

    public SheetPermission ToSheetPermissionEntity(ResultSet.Row row)
    {
        return new SheetPermission()
        {
            SheetId = row[UserAccessDbSettings.SheetIdField].GetUtf8(),
            PermissionId = row[UserAccessDbSettings.PermissionIdField].GetUtf8(),
        };
    }
}