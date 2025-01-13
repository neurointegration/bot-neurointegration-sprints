using Neurointegration.Api.DataModels.Models;

namespace Neurointegration.Api.Google;

public class GoogleSheetUtils
{
    public string GetAnswerCell(
        DateTime sprintStartDate,
        ScenarioType scenarioType,
        AnswerType answerType,
        DateOnly date,
        int sprintReplyNumber)
    {
        var dateOfSprint = date.DayNumber - DateOnly.FromDateTime(sprintStartDate).DayNumber;
        return scenarioType switch
        {
            ScenarioType.EveningStandUp => GetEveningStandUpCell(dateOfSprint, answerType),
            ScenarioType.Reflection => GetReflectionCell(sprintReplyNumber, answerType),
            ScenarioType.Status => GetStatusCell(dateOfSprint, sprintReplyNumber),
            _ => throw new ArgumentOutOfRangeException(nameof(scenarioType), scenarioType, null)
        };
    }

    public string GetDateStatusCell(int dayOfSprint)
    {
        var row = 12 + dayOfSprint;
        var column = "B";
        return $"Стендап!{column}{row}:{column}{row}";
    }

    private string GetEveningStandUpCell(int dayOfSprint, AnswerType answerType)
    {
        var row = 12 + dayOfSprint;
        var column = answerType switch
        {
            AnswerType.EveningStandUpWinnings => "F",
            AnswerType.EveningStandUpLive => "H",
            AnswerType.EveningStandUpPleasure => "L",
            AnswerType.EveningStandUpDrive => "P",
            _ => throw new ArgumentException($"Недопустимый тип ответа {answerType}")
        };

        return $"Стендап!{column}{row}:{column}{row}";
    }

    private string GetReflectionCell(int sprintReplyNumber, AnswerType answerType)
    {
        if (sprintReplyNumber == 3)
            return GetReflectionIntegrationCell(answerType);

        var row = answerType switch
        {
            AnswerType.ReflectionRegularWhatIDoing => "13",
            AnswerType.ReflectionRegularWhatINotDoing => "16",
            AnswerType.ReflectionRegularMyStatus => "19",
            AnswerType.ReflectionRegularOrbits => "22",
            AnswerType.ReflectionRegularCorrection => "25",
            _ => throw new ArgumentException($"Недопустимый тип ответа {answerType}")
        };
        var column = sprintReplyNumber switch
        {
            0 => "B",
            1 => "G",
            2 => "L",
            _ => throw new ArgumentException($"Недопустимый тип ответа {answerType}")
        };

        return $"Трекинг!{column}{row}:{column}{row}";
    }

    private string GetReflectionIntegrationCell(AnswerType answerType)
    {
        var row = answerType switch
        {
            AnswerType.ReflectionIntegrationChanges => "31",
            AnswerType.ReflectionIntegrationActions => "31",
            AnswerType.ReflectionIntegrationAbilities => "31",
            AnswerType.ReflectionIntegrationBeliefs => "34",
            AnswerType.ReflectionIntegrationSelfPerception => "34",
            AnswerType.ReflectionIntegrationOpportunities => "34",
            _ => throw new ArgumentException($"Недопустимый тип ответа {answerType}")
        };
        var column = answerType switch
        {
            AnswerType.ReflectionIntegrationChanges => "B",
            AnswerType.ReflectionIntegrationActions => "G",
            AnswerType.ReflectionIntegrationAbilities => "L",
            AnswerType.ReflectionIntegrationBeliefs => "B",
            AnswerType.ReflectionIntegrationSelfPerception => "G",
            AnswerType.ReflectionIntegrationOpportunities => "L",
            _ => throw new ArgumentException($"Недопустимый тип ответа {answerType}")
        };
        return $"Трекинг!{column}{row}:{column}{row}";
    }

    private string GetStatusCell(int dayOfSprint, int sprintReplyNumber)
    {
        var row = 12 + dayOfSprint;
        var answerNumber = sprintReplyNumber % 3;
        
        var column = answerNumber switch
        {
            0 => "C",
            1 => "D",
            2 => "E",
            _ => throw new ArgumentException("Недопустимый номер ответа")
        };

        return $"Стендап!{column}{row}:{column}{row}";
    }
}