using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.Settings;
using Ydb.Sdk.Value;

namespace Neurointegration.Api.Storages.Mapper;

public class QuestionMapper
{
    public Question ToQuestionEntity(ResultSet.Row row)
    {
        var scenarioType =
            Enum.TryParse<ScenarioType>(row[QuestionDbSettings.ScenarioTypeField].GetUtf8(), out var scenario);
        if (scenarioType == false)
            throw new ArgumentException();

        return new Question(
            row[QuestionDbSettings.DateField].GetDatetime(),
            row[QuestionDbSettings.UserIdField].GetInt64(),
            scenario,
            row[QuestionDbSettings.SprintNumberField].GetInt64(),
            row[QuestionDbSettings.SprintReplyNumberField].GetInt32(),
            row[QuestionDbSettings.PriorityField].GetInt32(),
            row[QuestionDbSettings.IsDelayedField].GetOptionalBool() ?? false);
    }
}