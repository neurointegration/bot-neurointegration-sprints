using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Result;
using Neurointegration.Api.Google;
using Neurointegration.Api.Storages;
using Neurointegration.Api.Storages.Answers;
using Neurointegration.Api.Storages.Sprints;

namespace Neurointegration.Api.Services;

public class AnswersService : IAnswersService
{
    private readonly IGoogleStorage googleStorage;
    private readonly GoogleSheetUtils googleSheetUtils;
    private readonly ISprintStorage sprintStorage;
    private readonly IAnswerStorage answersStorage;

    public AnswersService(
        IGoogleStorage googleStorage,
        GoogleSheetUtils googleSheetUtils,
        ISprintStorage sprintStorage,
        IAnswerStorage answersStorage)
    {
        this.googleStorage = googleStorage;
        this.googleSheetUtils = googleSheetUtils;
        this.sprintStorage = sprintStorage;
        this.answersStorage = answersStorage;
    }

    public async Task<Result> Save(SendAnswer answer)
    {
        await answersStorage.Save(answer);

        var getSprint = await sprintStorage.GetSprint(answer.UserId, answer.SprintNumber);

        if (!getSprint.IsSuccess)
            return getSprint;

        var range = googleSheetUtils.GetAnswerCell(getSprint.Value.SprintStartDate, answer.ScenarioType,
            answer.AnswerType, answer.Date, answer.SprintReplyNumber);

        await googleStorage.Save(answer.Answer, getSprint.Value.SheetId, range);

        return Result.Success();
    }
}