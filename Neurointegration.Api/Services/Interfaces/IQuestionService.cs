using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DataModels.Result;

namespace Neurointegration.Api.Services;

public interface IQuestionService
{
    Task<List<Question>> Get(int time, ScenarioType? scenarioType, long? userId);
    Task<Result> CreateDelayedQuestion(Question question);
}