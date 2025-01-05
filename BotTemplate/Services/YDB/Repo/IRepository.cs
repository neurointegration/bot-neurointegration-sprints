namespace BotTemplate.Services.YDB.Repo;

public interface IRepository
{
    Task ClearAll();
    Task CreateTable();
}