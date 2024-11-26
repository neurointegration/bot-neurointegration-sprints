using Neurointegration.Api.DataModels.Models;

namespace Neurointegration.Api.Storages.Questions;

public interface IQuestionStorage
{
    Task AddOrReplace(Question question);
    Task<IEnumerable<Question?>> Get(DateTime dateTime);
    Task<Question?> Get(long userId, ScenarioType scenarioType);
    Task Delete(Question? question);
    Task UpdateQuestion(Question question, Question updateQuestion);
    Task DeleteUserQuestions(long userId);
}