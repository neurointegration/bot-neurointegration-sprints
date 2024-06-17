using Ydb.Sdk.Value;

namespace BotTemplate.Services.YDB.Repo;

public class UsersRepo : IRepo
{
    protected virtual string TableName => "scenarios";

    private readonly IBotDatabase _botDatabase;

    private UsersRepo(IBotDatabase botDatabase)
    {
        this._botDatabase = botDatabase;
    }

    public static async Task<UsersRepo> InitWithDatabase(IBotDatabase botDatabase)
    {
        var model = new UsersRepo(botDatabase);
        await model.CreateTable();
        return model;
    }
    
    public async Task RegisterUser(long chatId)
    {
        if (await IsRegistered(chatId))
            return;
        
        await _botDatabase.ExecuteModify($@"
            DECLARE $chat_id AS Int64;

            INSERT INTO {TableName} ( chat_id )
            VALUES ( $chat_id )
        ", new Dictionary<string, YdbValue?>
        {
            {"$chat_id", YdbValue.MakeInt64(chatId)}
        });
    }
    
    public async Task<bool> IsRegistered(long chatId)
    {
        var rows = await _botDatabase.ExecuteFind($@"
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
            return false;
        }
        
        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return rowsArray.Any();
    }

    
    public async Task ClearAll()
    {
        await _botDatabase.ExecuteScheme($@"
            DROP TABLE {TableName};
        ");
    }

    public async Task CreateTable()
    {
        await _botDatabase.ExecuteScheme($@"
            CREATE TABLE {TableName} (
                chat_id Int64 NOT NULL,
                PRIMARY KEY (chat_id)
            )
        ");
    }
}