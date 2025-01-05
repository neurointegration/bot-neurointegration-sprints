using Common.Ydb;
using Neurointegration.Api.DataModels.Dto;
using Ydb.Sdk.Value;

namespace Neurointegration.Api.Storages.Answers;

public class AnswerStorage: IAnswerStorage
{
    private readonly YdbClient ydbClient;
    private readonly AnswerTableProperties tableProperties;

    public AnswerStorage(YdbClient ydbClient)
    {
        this.ydbClient = ydbClient;
        tableProperties = new AnswerTableProperties();
    }
    
    public async Task Save(SendAnswer answer)
    {
        await ydbClient.ExecuteModify($@"
            {tableProperties.Declare()}

             REPLACE INTO {tableProperties.TableName} 
                (   {tableProperties.Date.Name},
                    {tableProperties.UserId.Name},
                    {tableProperties.Answer.Name},
                    {tableProperties.ScenarioType.Name},
                    {tableProperties.AnswerNumber.Name}, 
                    {tableProperties.SprintReplyNumber.Name},
                    {tableProperties.SprintNumber.Name} )

             VALUES ( {tableProperties.Date.NameDeclare}, 
                        {tableProperties.UserId.NameDeclare}, 
                        {tableProperties.Answer.NameDeclare},
                        {tableProperties.ScenarioType.NameDeclare},
                        {tableProperties.AnswerNumber.NameDeclare},
                        {tableProperties.SprintReplyNumber.NameDeclare},
                        {tableProperties.SprintNumber.NameDeclare} )",
            new Dictionary<string, YdbValue>
            {
                {tableProperties.UserId.GetParameterKey(), tableProperties.UserId.GetParameterValue(answer.UserId)},
                {tableProperties.Date.GetParameterKey(), tableProperties.Date.GetParameterValue(answer.Date.ToDateTime(TimeOnly.MinValue))},
                {tableProperties.Answer.GetParameterKey(), tableProperties.Answer.GetParameterValue(answer.Answer)},
                {tableProperties.ScenarioType.GetParameterKey(), tableProperties.ScenarioType.GetParameterValue(answer.ScenarioType.ToString())},
                {tableProperties.AnswerNumber.GetParameterKey(), tableProperties.AnswerNumber.GetParameterValue(answer.AnswerNumber)},
                {tableProperties.SprintReplyNumber.GetParameterKey(), tableProperties.SprintReplyNumber.GetParameterValue(answer.SprintReplyNumber)},
                {tableProperties.SprintNumber.GetParameterKey(), tableProperties.SprintNumber.GetParameterValue(answer.SprintNumber)}
            });
    }
}