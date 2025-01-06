namespace Neurointegration.Api.DataModels.Models;

public record RoutineAction
{
    public RoutineType Type { get; set; }
    public string Action { get; set; }
}