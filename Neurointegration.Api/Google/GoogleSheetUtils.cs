using Neurointegration.Api.DataModels.Models;

namespace Neurointegration.Api.Google;

public class GoogleSheetUtils
{
    public string GetAnswerCell(
        DateTime sprintStartDate,
        ScenarioType scenarioType,
        int answerNumber,
        DateOnly date,
        int sprintReplyNumber)
    {
        var dateOfSprint = date.DayNumber - DateOnly.FromDateTime(sprintStartDate).DayNumber;
        return scenarioType switch
        {
            ScenarioType.EveningStandUp => GetEveningStandUpStatus(dateOfSprint, answerNumber),
            ScenarioType.Reflection => GetReflectionStatus(sprintReplyNumber, answerNumber),
            ScenarioType.Status => GetCellStatus(dateOfSprint, sprintReplyNumber),
            _ => throw new ArgumentOutOfRangeException(nameof(scenarioType), scenarioType, null)
        };
    }

    public string GetDateStatusCell(int dayOfSprint)
    {
        var row = 12 + dayOfSprint;
        var column = "B";
        return $"Стендап!{column}{row}:{column}{row}";
    }

    private string GetEveningStandUpStatus(int dayOfSprint, int answerNumber)
    {
        var row = 12 + dayOfSprint;
        var column = answerNumber switch
        {
            0 => "F",
            1 => "H",
            2 => "L",
            3 => "P",
            _ => throw new ArgumentException("Недопустимый номер ответа")
        };

        return $"Стендап!{column}{row}:{column}{row}";
    }

    private string GetReflectionStatus(int sprintReplyNumber, int answerNumber)
    {
        if (sprintReplyNumber == 3)
            return GetReflectionIntegrationStatus(answerNumber);

        var row = answerNumber switch
        {
            0 => "13",
            1 => "16",
            2 => "19",
            3 => "22",
            4 => "25",
            _ => throw new ArgumentException("Недопустимый номер ответа")
        };
        var column = sprintReplyNumber switch
        {
            0 => "B",
            1 => "G",
            2 => "L",
            _ => throw new ArgumentException("Недопустимый номер ответа")
        };

        return $"Трекинг!{column}{row}:{column}{row}";
    }

    private string GetReflectionIntegrationStatus(int answerNumber)
    {
        var row = answerNumber switch
        {
            0 => "31",
            1 => "31",
            2 => "31",
            3 => "34",
            4 => "34",
            5 => "34",
            _ => throw new ArgumentException("Недопустимый номер ответа")
        };
        var column = answerNumber switch
        {
            0 => "B",
            1 => "G",
            2 => "L",
            3 => "B",
            4 => "G",
            5 => "L",
            _ => throw new ArgumentException("Недопустимый номер ответа")
        };
        return $"Трекинг!{column}{row}:{column}{row}";
    }

    private string GetCellStatus(int dayOfSprint, int sprintReplyNumber)
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