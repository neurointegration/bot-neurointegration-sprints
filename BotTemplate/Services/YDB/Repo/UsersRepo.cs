using Ydb.Sdk.Value;

namespace BotTemplate.Services.YDB.Repo;

public class UsersRepo : IRepo
{
    protected virtual string TableName => "users_repo";

    private readonly IBotDatabase _botDatabase;

    private UsersRepo(IBotDatabase botDatabase)
    {
        _botDatabase = botDatabase;
    }

    public static async Task<UsersRepo> InitWithDatabase(IBotDatabase botDatabase)
    {
        var model = new UsersRepo(botDatabase);
        await model.CreateTable();
        return model;
    }

    public async Task RegisterUser(long chatId, bool iAmCoach, bool sendRegularMessages)
    {
        if (await IsRegistered(chatId))
            return;

        await _botDatabase.ExecuteModify($@"
            DECLARE $chat_id AS Int64;
            DECLARE $i_am_coach AS Text;
            DECLARE $send_regular_messages AS Text;

            INSERT INTO {TableName} ( chat_id, i_am_coach, send_regular_messages )
            VALUES ( $chat_id, $i_am_coach, $send_regular_messages );
        ", new Dictionary<string, YdbValue?>
        {
            { "$chat_id", YdbValue.MakeInt64(chatId) },
            { "$i_am_coach", YdbValue.MakeUtf8(iAmCoach.ToString()) },
            { "$send_regular_messages", YdbValue.MakeUtf8(sendRegularMessages.ToString()) }
        });
    }

    public async Task<bool> IsRegistered(long chatId)
    {
        var rows = await _botDatabase.ExecuteFind($@"
            DECLARE $chat_id AS Int64;

            SELECT chat_id
            FROM {TableName}
            WHERE chat_id = $chat_id
        ", new Dictionary<string, YdbValue>
        {
            { "$chat_id", YdbValue.MakeInt64(chatId) }
        });

        if (rows is null)
        {
            return false;
        }

        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return rowsArray.Any();
    }

    public async Task<bool> AmICoach(long chatId)
    {
        var rows = await _botDatabase.ExecuteFind($@"
            DECLARE $chat_id AS Int64;

            SELECT i_am_coach
            FROM {TableName}
            WHERE chat_id = $chat_id
        ", new Dictionary<string, YdbValue>
        {
            { "$chat_id", YdbValue.MakeInt64(chatId) }
        });

        if (rows is null)
        {
            return false;
        }

        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return rowsArray.Any(row => bool.Parse(row["i_am_coach"].GetUtf8()));
    }

    public async Task ChangeIAmCoach(long chatId, bool value)
    {
        await _botDatabase.ExecuteModify($@"
            DECLARE $chat_id AS Int64;
            DECLARE $i_am_coach AS Text;

            UPDATE {TableName}
            SET i_am_coach = $i_am_coach
            WHERE chat_id = $chat_id;
        ", new Dictionary<string, YdbValue>
        {
            { "$chat_id", YdbValue.MakeInt64(chatId) },
            { "$i_am_coach", YdbValue.MakeUtf8(value.ToString()) }
        });
    }

    public async Task<bool> SendRegularMessages(long chatId)
    {
        var rows = await _botDatabase.ExecuteFind($@"
            DECLARE $chat_id AS Int64;

            SELECT send_regular_messages
            FROM {TableName}
            WHERE chat_id = $chat_id
        ", new Dictionary<string, YdbValue>
        {
            { "$chat_id", YdbValue.MakeInt64(chatId) }
        });

        if (rows is null)
        {
            return false;
        }

        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return rowsArray.Any(row => bool.Parse(row["send_regular_messages"].GetUtf8()));
    }

    public async Task ChangeSendRegularMessages(long chatId, bool value)
    {
        await _botDatabase.ExecuteModify($@"
            DECLARE $chat_id AS Int64;
            DECLARE $send_regular_messages AS Text;

            UPDATE {TableName}
            SET send_regular_messages = $send_regular_messages
            WHERE chat_id = $chat_id;
        ", new Dictionary<string, YdbValue>
        {
            { "$chat_id", YdbValue.MakeInt64(chatId) },
            { "$send_regular_messages", YdbValue.MakeUtf8(value.ToString()) }
        });
    }

    public async Task ClearAll()
    {
        await _botDatabase.ExecuteScheme($@"
            DROP TABLE {TableName};
        ");
    }

    public async Task CreateTable()
    {
        try
        {
            await _botDatabase.ExecuteScheme($@"
            CREATE TABLE {TableName} (
                chat_id Int64 NOT NULL,
                i_am_coach Text NOT NULL,
                send_regular_messages Text NOT NULL,
                PRIMARY KEY (chat_id)
            )
        ");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}