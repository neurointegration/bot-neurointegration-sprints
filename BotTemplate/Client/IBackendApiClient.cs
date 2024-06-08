using BotTemplate.Models.ClientDto;

namespace BotTemplate.Client;

public interface IBackendApiClient
{
    /// <summary>
    /// Сохранить ответ на вопрос
    /// </summary>
    /// <param name="sendAnswer">Ответ</param>
    Task SendAnswerAsync(SendAnswer sendAnswer);

    /// <summary>
    /// Достать список вопросов
    /// </summary>
    /// <param name="timePeriod">Промежуток за который взять вопросы</param>
    Task<List<Question>?> GetQuestionsAsync(int timePeriod);

    /// <summary>
    /// Создать пользователя
    /// </summary>
    /// <param name="createUser">Данные пользователя</param>
    Task<ApiUser?> CreateUserAsync(CreateUser createUser);

    /// <summary>
    /// Обновить пользователя, указываются только обновляемые поля, если поле обновлять не надо, то оно указывается как null
    /// </summary>
    /// <param name="updateUser">Новые данные пользователя</param>
    Task<ApiUser?> UpdateUserAsync(UpdateUser updateUser);

    /// <summary>
    /// Достать гугл таблицы пользователя
    /// </summary>
    /// <param name="ownerId">id чьи гугл таблицы хотим достать</param>
    /// <param name="grantedUserId">id запрашивающего таблицы</param>
    Task<List<string>?> GetUserSpreadSheetsAsync(long ownerId, long grantedUserId);

    /// <summary>
    /// Выдать права пользавателю к информации другого пользователя
    /// </summary>
    /// <param name="ownerId">id того к данным кого даем доступ</param>
    /// <param name="grantedUserId">id того, кто эти права должен получить</param>
    Task GrantedAccessToUserInfoAsync(long ownerId, long grantedUserId);

    /// <summary>
    /// Получить список доступных пользователей
    /// </summary>
    Task<List<ApiUser>?> GetPublicCoachsAsync();
}