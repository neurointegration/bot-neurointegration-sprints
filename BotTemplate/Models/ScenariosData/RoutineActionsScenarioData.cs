using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Models.ScenariosData;

public record RoutineActionsScenarioData
{
    public int MessageId { get; set; }
    public RoutineType? RoutineType { get; set; }
}