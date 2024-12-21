using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DataModels.Result;
using Neurointegration.Api.Excpetions;
using Neurointegration.Api.Settings;
using Neurointegration.Api.Storages.Mapper;
using Ydb.Sdk.Value;

namespace Neurointegration.Api.Storages.Questions;

public class QuestionStorage : IQuestionStorage
{
    private readonly YdbClient ydbClient;
    private readonly QuestionMapper questionMapper;

    public QuestionStorage(YdbClient ydbClient, QuestionMapper questionMapper)
    {
        this.ydbClient = ydbClient;
        this.questionMapper = questionMapper;
    }

    public async Task AddOrReplace(Question question)
    {
        await ydbClient.ExecuteModify($@"
            DECLARE ${QuestionDbSettings.DateField} AS DATETIME;
            DECLARE ${QuestionDbSettings.UserIdField} AS Int64;
            DECLARE ${QuestionDbSettings.ScenarioTypeField} AS Utf8;
            DECLARE ${QuestionDbSettings.PriorityField} AS Int32;
            DECLARE ${QuestionDbSettings.SprintReplyNumberField} AS Int32;
            DECLARE ${QuestionDbSettings.SprintNumberField} AS Int64;

             REPLACE INTO {QuestionDbSettings.TableName} 
                ( {QuestionDbSettings.DateField}, {QuestionDbSettings.UserIdField},
                  {QuestionDbSettings.ScenarioTypeField}, {QuestionDbSettings.PriorityField}, 
                    {QuestionDbSettings.SprintReplyNumberField}, {QuestionDbSettings.SprintNumberField} )
             VALUES ( ${QuestionDbSettings.DateField}, ${QuestionDbSettings.UserIdField},
                      ${QuestionDbSettings.ScenarioTypeField}, ${QuestionDbSettings.PriorityField},
                      ${QuestionDbSettings.SprintReplyNumberField}, ${QuestionDbSettings.SprintNumberField} )",
            new Dictionary<string, YdbValue>
            {
                {$"${QuestionDbSettings.DateField}", YdbValue.MakeDatetime(question.Date)},
                {$"${QuestionDbSettings.UserIdField}", YdbValue.MakeInt64(question.UserId)},
                {$"${QuestionDbSettings.ScenarioTypeField}", YdbValue.MakeUtf8(question.ScenarioType.ToString())},
                {$"${QuestionDbSettings.PriorityField}", YdbValue.MakeInt32(question.Priority)},
                {$"${QuestionDbSettings.SprintReplyNumberField}", YdbValue.MakeInt32(question.SprintReplyNumber)},
                {$"${QuestionDbSettings.SprintNumberField}", YdbValue.MakeInt64(question.SprintNumber)}
            });
    }

    public async Task<IEnumerable<Question>> Get(DateTime dateTime, ScenarioType? scenarioType = null)
    {
        if (scenarioType != null)
            return await GetByScenario(dateTime, scenarioType.Value);
        
        var rows = await ydbClient.ExecuteFind($@"
            DECLARE ${QuestionDbSettings.DateField} AS DATETIME;

            SELECT *
            FROM {QuestionDbSettings.TableName}
            WHERE {QuestionDbSettings.DateField} <= ${QuestionDbSettings.DateField}",
            new Dictionary<string, YdbValue>
            {
                {$"${QuestionDbSettings.DateField}", YdbValue.MakeDatetime(dateTime)}
            });

        return rows.Select(row => questionMapper.ToQuestionEntity(row));
    }
    
    public async Task<IEnumerable<Question>> GetByScenario(DateTime dateTime, ScenarioType scenarioType)
    {
        var rows = await ydbClient.ExecuteFind($@"
            DECLARE ${QuestionDbSettings.DateField} AS DATETIME;
            DECLARE ${QuestionDbSettings.ScenarioTypeField} AS Utf8;

            SELECT *
            FROM {QuestionDbSettings.TableName}
            WHERE {QuestionDbSettings.DateField} <= ${QuestionDbSettings.DateField}
                    AND {QuestionDbSettings.ScenarioTypeField} = ${QuestionDbSettings.ScenarioTypeField}",
            new Dictionary<string, YdbValue>
            {
                {$"${QuestionDbSettings.DateField}", YdbValue.MakeDatetime(dateTime)},
                {$"${QuestionDbSettings.ScenarioTypeField}", YdbValue.MakeUtf8(scenarioType.ToString())}
            });

        return rows.Select(row => questionMapper.ToQuestionEntity(row));
    }


    public async Task<Result<Question>> Get(long userId, ScenarioType scenarioType)
    {
        var rows = await ydbClient.ExecuteFind($@"
            DECLARE ${QuestionDbSettings.UserIdField} AS Int64;
            DECLARE ${QuestionDbSettings.ScenarioTypeField} AS Utf8;

            SELECT *
            FROM {QuestionDbSettings.TableName}
            WHERE {QuestionDbSettings.UserIdField} = ${QuestionDbSettings.UserIdField} AND 
                  {QuestionDbSettings.ScenarioTypeField} = ${QuestionDbSettings.ScenarioTypeField}",
            new Dictionary<string, YdbValue>
            {
                {$"${QuestionDbSettings.UserIdField}", YdbValue.MakeInt64(userId)},
                {$"${QuestionDbSettings.ScenarioTypeField}", YdbValue.MakeUtf8(scenarioType.ToString())}
            });
        
        var rowsList = rows.ToList();
        if (rowsList.Count == 0)
            return Result<Question>.Fail(Error.NotFound($"Не удалось найти вопрос по userId={userId} и scenarioType={scenarioType}"));

        return Result<Question>.Success(questionMapper.ToQuestionEntity(rowsList[0]));
    }

    public async Task<Result> Delete(Question question)
    {
        try
        {
            await ydbClient.ExecuteModify($@"
            DECLARE ${QuestionDbSettings.DateField} AS DATETIME;
            DECLARE ${QuestionDbSettings.UserIdField} AS Int64;

            DELETE FROM {QuestionDbSettings.TableName}
            WHERE {QuestionDbSettings.DateField} == ${QuestionDbSettings.DateField} AND 
                  {QuestionDbSettings.UserIdField} == ${QuestionDbSettings.UserIdField};",
                new Dictionary<string, YdbValue>
                {
                    {$"${QuestionDbSettings.DateField}", YdbValue.MakeDatetime(question.Date)},
                    {$"${QuestionDbSettings.UserIdField}", YdbValue.MakeInt64(question.UserId)}
                });
        }
        catch (Exception e)
        {
            return Result.Fail(Error.InnerError($"Не удалось удалить вопрос {question}. Ошибка {e.Message}"));
        }

        return Result.Success();
    }

    public async Task UpdateQuestion(Question question, Question updateQuestion)
    {
        await AddOrReplace(updateQuestion);
        await Delete(question);
    }

    public async Task DeleteUserQuestions(long userId)
    {
        await ydbClient.ExecuteModify($@"
            DECLARE ${QuestionDbSettings.UserIdField} AS Int64;

            DELETE FROM {QuestionDbSettings.TableName}
            WHERE {QuestionDbSettings.UserIdField} == ${QuestionDbSettings.UserIdField};",
            new Dictionary<string, YdbValue>
            {
                {$"${QuestionDbSettings.UserIdField}", YdbValue.MakeInt64(userId)}
            });
    }
}