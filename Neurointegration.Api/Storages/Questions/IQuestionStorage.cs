using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DataModels.Result;

namespace Neurointegration.Api.Storages.Questions;

public interface IQuestionStorage
{
    Task AddOrReplace(Question question);
    Task<IEnumerable<Question>> Get(DateTime dateTime, ScenarioType? scenarioType = null);
    Task<Result<Question>> Get(long userId, ScenarioType scenarioType);
    Task<Result> Delete(Question question);
    Task UpdateQuestion(Question question, Question updateQuestion);
    Task DeleteUserQuestions(long userId);
}