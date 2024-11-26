namespace Neurointegration.Api.Storages.Tables.Ydb;

public interface ITableInitializer
{
    Task CreateTable();
}