using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DataModels.Result;

namespace Neurointegration.Api.Services.Decorators;

public class QuestionServiceDecorator: IQuestionService
{
    private readonly IQuestionService questionService;
    private readonly ILogger log;

    public QuestionServiceDecorator(IQuestionService questionService, ILogger log)
    {
        this.questionService = questionService;
        this.log = log;
    }

    public async Task<Result> CreateDelayedQuestion(Question question)
    {
        log.LogInformation($"Отложение вопроса {question} на 1 час");
        var getResult = await questionService.CreateDelayedQuestion(question);
        
        log.LogInformation($"Откладывание вопроса завершено");

        return getResult;
    }

    public async Task<List<Question>> Get(int time, ScenarioType? scenarioType, long? userId)
    {
        log.LogInformation($"Получаем вопросы на следующие {time} минут и по сценарию {scenarioType}");
        var getResult = await questionService.Get(time, scenarioType, userId);
        
        log.LogInformation($"Получено {getResult.Count} вопросов");

        return getResult;
    }
}