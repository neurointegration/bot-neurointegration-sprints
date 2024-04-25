using System.Text;
using Ydb.Sdk.Value;

namespace BotTemplate.Services.YDB.Repo;

public class UserAnswersRepo : IRepo
{
    protected virtual string TableName => "user_answers";

    private readonly IBotDatabase botDatabase;

    private UserAnswersRepo(IBotDatabase botDatabase)
    {
        this.botDatabase = botDatabase;
    }

    public static async Task<UserAnswersRepo> InitWithDatabase(IBotDatabase botDatabase)
    {
        var model = new UserAnswersRepo(botDatabase);
        await model.CreateTable();
        return model;
    }
    
    public async Task SaveAnswer(long chatId, string key, string text)
    {
        var newId = await GetUserAnswerIdOrNull();
        if (newId is null)
            newId = 1;
        else
            newId++;
        
        await botDatabase.ExecuteModify($@"
            DECLARE $pk AS Int64;
            DECLARE $chat_id AS Int64;
            DECLARE $key AS text;
            DECLARE $answer AS text;

            INSERT INTO {TableName} ( pk, chat_id, key, answer )
            VALUES ( $pk, $chat_id, $key, $answer )
        ", new Dictionary<string, YdbValue?>
        {
            {"$pk", YdbValue.MakeInt64(newId!.Value)},
            {"$chat_id", YdbValue.MakeInt64(chatId)},
            {"$key", YdbValue.MakeUtf8(key)},
            {"$answer", YdbValue.MakeUtf8(text)}
        });
    }
    
    public async Task<string[]> GetAll(long chatId)
    {
        var rows = await botDatabase.ExecuteFind($@"
            DECLARE $chat_id AS Int64;

            SELECT answer
            FROM {TableName}
            WHERE chat_id = $chat_id
        ", new Dictionary<string, YdbValue>
        {
            {"$chat_id", YdbValue.MakeInt64(chatId)}
        });
        
        if (rows is null)
        {
            return Array.Empty<string>();
        }
        
        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return !rowsArray.Any() 
            ? Array.Empty<string>() 
            : rowsArray.Select(message => message["answer"].GetUtf8()).ToArray();
    }

    private async Task<long?> GetUserAnswerIdOrNull()
    {
        var rows = await botDatabase.ExecuteFind($@"
            SELECT MAX(pk) max_pk
            FROM {TableName}
        ", new Dictionary<string, YdbValue>());
        
        if (rows is null)
        {
            return null;
        }
        
        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return !rowsArray.Any() 
            ? null 
            : rowsArray.First()["max_pk"].GetOptionalInt64();
    }

    public async Task ClearAll()
    {
        await botDatabase.ExecuteScheme($@"
            DROP TABLE {TableName};
        ");
    }

    public async Task CreateTable()
    {
        await botDatabase.ExecuteScheme($@"
            CREATE TABLE {TableName} (
                pk Int64 NOT NULL,
                chat_id Int64 NOT NULL,
                key text NOT NULL,
                answer text NOT NULL,
                PRIMARY KEY (pk)
            )
        ");
    }
}