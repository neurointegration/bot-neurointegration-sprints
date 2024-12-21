using Neurointegration.Api.DataModels.Models;

namespace Neurointegration.Api.Services;

public interface IQuestionService
{
    Task<List<Question>> Get(int time, ScenarioType? scenarioType);
}