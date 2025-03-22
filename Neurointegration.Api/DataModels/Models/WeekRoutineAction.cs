namespace Neurointegration.Api.DataModels.Models;

public record WeekRoutineAction
{
    public RoutineType Type { get; set; }
    public string Action { get; set; }
    public string ActionId { get; set; }
    public int WeekCount { get; set; }
}