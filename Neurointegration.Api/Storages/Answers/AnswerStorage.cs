using Common.Ydb;
using Neurointegration.Api.DataModels.Dto;
using Ydb.Sdk.Value;

namespace Neurointegration.Api.Storages.Answers;

public class AnswerStorage : IAnswerStorage
{
    private readonly YdbClient ydbClient;
    private readonly AnswerTableSchema tableSchema;

    public AnswerStorage(YdbClient ydbClient, AnswerTableSchema tableSchema)
    {
        this.ydbClient = ydbClient;
        this.tableSchema = tableSchema;
    }

    public async Task Save(SendAnswer answer)
    {
        await ydbClient.Replace(
            tableSchema,
            new[]
            {
                tableSchema.UserId.WithValue(answer.UserId),
                tableSchema.Date.WithValue(answer.Date.ToDateTime(TimeOnly.MinValue)),
                tableSchema.Answer.WithValue(answer.Answer),
                tableSchema.ScenarioType.WithValue(answer.ScenarioType.ToString()),
                tableSchema.AnswerNumber.WithValue(answer.AnswerNumber),
                tableSchema.SprintReplyNumber.WithValue(answer.SprintReplyNumber),
                tableSchema.SprintNumber.WithValue(answer.SprintNumber)
            });
    }
}