namespace Neurointegration.Api.DataModels.Models;

public record RoutineAction
{
    public RoutineType Type { get; set; }
    public string Action { get; set; }

    public RoutineAction(RoutineType type, string action)
    {
        Type = type;
        Action = action;
    }
    
    protected RoutineAction()
    {
        
    }
}