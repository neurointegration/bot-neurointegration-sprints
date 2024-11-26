namespace BotTemplate.Services.YDB.Repo;

public interface IRepo
{
    Task ClearAll();
    Task CreateTable();
}