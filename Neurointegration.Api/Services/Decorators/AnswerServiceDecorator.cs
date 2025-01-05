using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Result;

namespace Neurointegration.Api.Services.Decorators;

public class AnswerServiceDecorator : IAnswersService
{
    private readonly IAnswersService answersService;
    private readonly ILogger log;

    public AnswerServiceDecorator(IAnswersService answersService, ILogger log)
    {
        this.answersService = answersService;
        this.log = log;
    }

    public async Task<Result> Save(SendAnswer answer)
    {
        log.LogInformation($"Сохраняем ответ {answer}");
        var saveResult = await answersService.Save(answer);
        
        if (!saveResult.IsSuccess)
            log.LogWarning($"Неудачная попытка сохранения. Ошибка ${saveResult.Error}");
        
        return saveResult;
    }
}