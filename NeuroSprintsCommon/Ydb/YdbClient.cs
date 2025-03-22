using Common.Ydb.Fields;
using Common.Ydb.Schema;
using Microsoft.Extensions.Logging;
using Ydb.Sdk;
using Ydb.Sdk.Auth;
using Ydb.Sdk.Client;
using Ydb.Sdk.Services.Table;
using Ydb.Sdk.Value;
using Ydb.Sdk.Yc;

namespace Common.Ydb;

public class YdbClient
{
    private readonly YdbSecretSettings configuration;
    private readonly ILogger logger;

    public YdbClient(YdbSecretSettings configuration, ILogger logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task Replace(
        ITableSchema tableSchema,
        IEnumerable<YdbFieldWithValue> fields)
    {
        fields = fields.ToArray();

        var replaceFields = fields
            .Select(field => $"{field.Name}");

        var replaceValuesDeclare = fields
            .Select(field => $"{field.NameDeclare}");

        var parameters = fields
            .ToDictionary(filed => filed.NameDeclare, field => field.Value);

        var query = $@"
            {tableSchema.DeclareFields()}

             REPLACE INTO {tableSchema.TableName} 
                ( {string.Join(", ", replaceFields)} )

             VALUES ( {string.Join(", ", replaceValuesDeclare)} )";

        await ExecuteDataQuery(query, parameters);
    }

    public async Task Update(
        ITableSchema tableSchema,
        IEnumerable<YdbFieldWithValue> conditionFields,
        IEnumerable<YdbFieldWithValue> newFields)
    {
        conditionFields = conditionFields.ToArray();
        newFields = newFields.ToArray();

        var whereFields = conditionFields
            .Select(field => $"{field.Name} = {field.NameDeclare}");

        var setFields = newFields
            .Select(field => $"{field.Name} = {field.NameDeclare}");

        var query = $@"
            {tableSchema.DeclareFields()}

            UPDATE {tableSchema.TableName}
            SET {string.Join(", ", setFields)}
            WHERE {string.Join(" AND ", whereFields)}";

        var parameters = conditionFields
            .Concat(newFields)
            .ToDictionary(filed => filed.NameDeclare, field => field.Value);

        await ExecuteDataQuery(query, parameters);
    }

    public async Task Delete(ITableSchema tableSchema, YdbFieldWithValue[] conditionFields)
    {
        conditionFields = conditionFields.ToArray();

        var whereFields = conditionFields
            .Select(field => $"{field.Name} = {field.NameDeclare}");

        var query = $@"
            {tableSchema.DeclareFields()}

            DELETE FROM {tableSchema.TableName} 
            WHERE {string.Join(" AND ", whereFields)}";

        var parameters = conditionFields
            .ToDictionary(filed => filed.NameDeclare, field => field.Value);

        await ExecuteDataQuery(query, parameters);
    }

    public async Task<IEnumerable<ResultSet.Row>> Find(
        ITableSchema tableSchema,
        YdbField[] selectFields,
        YdbFieldWithValue[] conditionFields)
    {
        conditionFields = conditionFields.ToArray();

        var whereFields = conditionFields
            .Select(field => $"{field.Name} = {field.NameDeclare}");

        var selectFieldsInner = selectFields
            .Select(field => $"{field.Name}");

        var query = $@"
            {tableSchema.DeclareFields()}

            SELECT {string.Join(", ", selectFieldsInner)}
            FROM {tableSchema.TableName}
            WHERE {string.Join(" AND ", whereFields)}
        ";

        var parameters = conditionFields
            .ToDictionary(filed => filed.NameDeclare, field => field.Value);

        return await ExecuteFind(query, parameters);
    }

    public async Task CreateTable(ITableSchema tableSchema)
    {
        var primaryKey = tableSchema.Fields
            .Where(field => field.Conditions.Contains(FieldConditions.IsPrimaryKey))
            .Select(field => field.Name);

        var createFields = tableSchema.Fields
            .Select(field =>
                $"{field.Name} {field.Type} {(field.Conditions.Contains(FieldConditions.NotNull) ? "NOT NULL" : "")}");

        var query = $@"
             CREATE TABLE {tableSchema.TableName} (
                {string.Join(", ", createFields)},
                PRIMARY KEY ({string.Join(", ", primaryKey)})
             )
         ";

        using var tableClient = await CreateTableClient();

        var response = await tableClient.SessionExec(async session
            => await session.ExecuteSchemeQuery(query)
        );

        if (response.Status.Issues.Count != 0)
            throw new Exception(string.Join("; ", response.Status.Issues));

        response.Status.EnsureSuccess();
    }

    public async Task<IEnumerable<ResultSet.Row>> ExecuteFind(
        string query, Dictionary<string, YdbValue> parameters)
    {
        var response = await ExecuteDataQuery(query, parameters);
        var queryResponse = (ExecuteDataQueryResponse) response;

        return queryResponse.Result.ResultSets.Count == 0
            ? ArraySegment<ResultSet.Row>.Empty
            : queryResponse.Result.ResultSets[0].Rows;
    }

    public async Task ExecuteScheme(string query)
    {
        using var tableClient = await CreateTableClient();

        var response = await tableClient.SessionExec(async session
            => await session.ExecuteSchemeQuery(query)
        );

        response.Status.EnsureSuccess();
    }

    public async Task<IResponse> ExecuteDataQuery(string query, Dictionary<string, YdbValue> parameters)
    {
        using var tableClient = await CreateTableClient();

        logger.LogInformation(query);
        logger.LogInformation(string.Join(";", parameters));

        var response = await tableClient.SessionExec(async session
            => await session.ExecuteDataQuery(
                query,
                TxControl.BeginSerializableRW().Commit(),
                parameters)
        );

        // if (response.Status.Issues.Where(x=>x.Message.Contains()).Count != 0)
        // {
        //     logger.LogInformation($"Проблемы при выполнение запроса в бд {string.Join("; ", response.Status.Issues)}");
        //     throw new Exception(string.Join("; ", response.Status.Issues));
        // }

        response.Status.EnsureSuccess();

        return response;
    }

    private async Task<TableClient> CreateTableClient()
    {
        ICredentialsProvider provider;

        if (string.IsNullOrEmpty(configuration.IamTokenPath))
        {
            provider = new MetadataProvider();
            await ((MetadataProvider) provider).Initialize();
        }
        else
        {
            provider = new ServiceAccountProvider(configuration.IamTokenPath);
        }

        var config = new DriverConfig(
            configuration.YdbEndpoint,
            configuration.YdbPath,
            provider
        );

        var driver = new Driver(config);
        await driver.Initialize();

        return new TableClient(driver, new TableClientConfig());
    }
}