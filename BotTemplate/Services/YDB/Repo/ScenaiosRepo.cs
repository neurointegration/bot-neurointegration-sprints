using System.Text;
using Ydb.Sdk.Value;

namespace BotTemplate.Services.YDB.Repo;

public class ScenaiosRepo
{
    public virtual string TableName => "scenarios";

    private readonly IBotDatabase botDatabase;

    public ScenaiosRepo(IBotDatabase botDatabase)
    {
        this.botDatabase = botDatabase;
    }

    public static async Task<ScenaiosRepo> InitWithDatabase(IBotDatabase botDatabase)
    {
        var model = new ScenaiosRepo(botDatabase);
        await model.CreateTable();
        return model;
    }
    
    public async Task<string?> GetMessageByScenarioIdAndMessageIndex(long scenarioId, int messageIndex)
    {
        var rows = await botDatabase.ExecuteFind($@"
            DECLARE $scenario_id AS Int64;
            DECLARE $message_index AS Int32;

            SELECT message
            FROM {TableName}
            WHERE $scenario_id = $scenario_id AND $message_index = $message_index
        ", new Dictionary<string, YdbValue>
        {
            {"$scenario_id", YdbValue.MakeInt64(scenarioId)},
            {"$message_index", YdbValue.MakeInt32(messageIndex)}
        });
        
        if (rows is null)
        {
            return null;
        }
        
        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return !rowsArray.Any() 
            ? null 
            : Encoding.Default.GetString(rowsArray.First()["message"].GetString());
    }
    
    public async Task<string[]> GetAllMessagesByScenarioId(long scenarioId)
    {
        var rows = await botDatabase.ExecuteFind($@"
            DECLARE $scenario_id AS Int64;

            SELECT message
            FROM {TableName}
            WHERE $scenario_id = $scenario_id
        ", new Dictionary<string, YdbValue>
        {
            {"$scenario_id", YdbValue.MakeInt64(scenarioId)}
        });
        
        if (rows is null)
        {
            return Array.Empty<string>();
        }
        
        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return !rowsArray.Any() 
            ? Array.Empty<string>() 
            : rowsArray.Select(message => Encoding.Default.GetString(message["message"].GetString())).ToArray();
    }

    public async void AddNewScenario(string[] messages)
    {
        var maxId = await GetMaxScenarioIdOrNull();
        var scenarioId = maxId is not null 
            ? maxId.Value + 1 
            : 1;
        
        for (var i = 0; i < messages.Length; i++)
        {
            await botDatabase.ExecuteModify($@"
            DECLARE $scenario_id AS Int64;
            DECLARE $message_index AS Int32;
            DECLARE $message AS text;

            INSERT INTO {TableName} ( scenario_id, message_index, message )
            VALUES ( $scenario_id, $message_index, $message )
        ", new Dictionary<string, YdbValue>
            {
                { "$scenario_id", YdbValue.MakeInt64(scenarioId) },
                { "$message_index", YdbValue.MakeInt32(i) },
                { "$date_time", YdbValue.MakeString(Encoding.Default.GetBytes(messages[i])) }
            });
        }
    }

    private async Task<long?> GetMaxScenarioIdOrNull()
    {
        var rows = await botDatabase.ExecuteFind($@"
            SELECT MAX(scenario_id)
            FROM {TableName}
        ", new Dictionary<string, YdbValue>());
        
        if (rows is null)
        {
            return null;
        }
        
        var rowsArray = rows as ResultSet.Row[] ?? rows.ToArray();

        return !rowsArray.Any() 
            ? null 
            : rowsArray.First()["scenario_id"].GetInt64();
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
                scenario_id Int64 NOT NULL,
                message_index Int32 NOT NULL,
                message text,
                PRIMARY KEY (scenario_id, message_index)
            )
        ");
    }
}