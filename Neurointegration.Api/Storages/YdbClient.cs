using Neurointegration.Api.Settings;
using Ydb.Sdk;
using Ydb.Sdk.Auth;
using Ydb.Sdk.Services.Table;
using Ydb.Sdk.Value;
using Ydb.Sdk.Yc;

namespace Neurointegration.Api.Storages;

public class YdbClient
{
    private readonly ApiSecretSettings configuration;

    public YdbClient(ApiSecretSettings configuration)
    {
        this.configuration = configuration;
    }

    public async Task<IEnumerable<ResultSet.Row>> ExecuteFind(
        string query, Dictionary<string, YdbValue> parameters)
    {
        using var tableClient = await CreateTableClient();

        var response = await tableClient.SessionExec(async session
            => await session.ExecuteDataQuery(
                query,
                TxControl.BeginSerializableRW().Commit(),
                parameters)
        );

        response.Status.EnsureSuccess();
        var queryResponse = (ExecuteDataQueryResponse) response;

        return queryResponse.Result.ResultSets.Count == 0
            ? ArraySegment<ResultSet.Row>.Empty
            : queryResponse.Result.ResultSets[0].Rows;
    }

    public async Task ExecuteModify(string query, Dictionary<string, YdbValue> parameters)
    {
        using var tableClient = await CreateTableClient();

        var response = await tableClient.SessionExec(async session
            => await session.ExecuteDataQuery(
                query,
                TxControl.BeginSerializableRW().Commit(),
                parameters)
        );

        response.Status.EnsureSuccess();
    }

    public async Task ExecuteScheme(string query)
    {
        using var tableClient = await CreateTableClient();

        var response = await tableClient.SessionExec(async session
            => await session.ExecuteSchemeQuery(query)
        );

        response.Status.EnsureSuccess();
    }

    private async Task<TableClient> CreateTableClient()
    {
        ICredentialsProvider provider;

        // && string.IsNullOrEmpty(configuration.TokenJson)
        if (string.IsNullOrEmpty(configuration.IamTokenPath))
        {
            provider = new MetadataProvider();
            await ((MetadataProvider) provider).Initialize();
        }
        // else if (!string.IsNullOrEmpty(configuration.TokenJson))
        // {
        //     provider = new MetadataProvider();
        // }
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