using Neurointegration.Api.Storages.Tables;

namespace Neurointegration.Api.Extensions;

public static class ApplicationExtensions
{
    public static async Task UseDb(this WebApplication app)
    {
        await app.Services.GetRequiredService<YdbInitializer>().CreateTables();
    }
}