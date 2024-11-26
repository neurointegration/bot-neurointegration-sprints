using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.Excpetions;
using Neurointegration.Api.Google;
using Neurointegration.Api.Storages;
using Neurointegration.Api.Storages.Sprints;

namespace Neurointegration.Api.Services;

public class AnswersService : IAnswersService
{
    private readonly IGoogleStorage googleStorage;
    private readonly GoogleSheetUtils googleSheetUtils;
    private readonly ISprintStorage sprintStorage;

    public AnswersService(
        IGoogleStorage googleStorage,
        GoogleSheetUtils googleSheetUtils,
        ISprintStorage sprintStorage)
    {
        this.googleStorage = googleStorage;
        this.googleSheetUtils = googleSheetUtils;
        this.sprintStorage = sprintStorage;
    }

    public async Task Save(SendAnswer answer)
    {
        var sprint = await sprintStorage.GetSprint(answer.UserId, answer.SprintNumber);
        
        if (sprint == null)
            throw new ArgumentException($"У пользователя id={answer.UserId} нет информации о спринтах");
        
        var range = googleSheetUtils.GetAnswerCell(sprint.SprintStartDate, answer.ScenarioType, answer.AnswerNumber, answer.Date, answer.SprintReplyNumber);

        await googleStorage.Save(answer.Answer, sprint.SheetId, range);
    }
}