namespace Neurointegration.Api.DataModels.Models;

public enum ScenarioType
{
    EveningStandUp,
    Reflection,
    Status
}

public static class ScenarioTypeExtension
{
    private static HashSet<ScenarioType> regularEvents = new HashSet<ScenarioType>()
        {ScenarioType.EveningStandUp, ScenarioType.Reflection, ScenarioType.Status};

    public static bool IsRegularEvent(this ScenarioType scenarioType)
    {
        return regularEvents.Contains(scenarioType);
    }
}