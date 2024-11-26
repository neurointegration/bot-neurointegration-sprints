using Neurointegration.Api.DataModels.Models;

namespace Neurointegration.Api.Services;

public interface IQuestionService
{
    Task CreateQuestion(Question question);
    Task<List<Question>> Get(int time);
}