using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;

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
    Task<List<Question>> GetQuestionsAsync(int timePeriod, ScenarioType? scenarioType);

    /// <summary>
    /// Создать пользователя
    /// </summary>
    /// <param name="createUser">Данные пользователя</param>
    Task<User> CreateUserAsync(CreateUser createUser);

    /// <summary>
    /// Обновить пользователя, указываются только обновляемые поля, если поле обновлять не надо, то оно указывается как null
    /// </summary>
    /// <param name="updateUser">Новые данные пользователя</param>
    Task<User?> UpdateUserAsync(UpdateUser updateUser);

    /// <summary>
    /// Достать информацию о пользователе
    /// </summary>
    /// <param name="userId">Id пользователя</param>
    Task<User> GetUserAsync(long userId);

    /// <summary>
    /// Получить все спринты пользователя
    /// </summary>
    /// <param name="ownerId">id чьи спринты хотим достать</param>
    /// <param name="grantedUserId">id запрашивающего</param>
    /// <returns></returns>
    Task<List<Sprint>> GetUserSprintsAsync(long ownerId, long grantedUserId);

    // /// <summary>
    // /// Достать гугл таблицы пользователя
    // /// </summary>
    // /// <param name="ownerId">id чьи гугл таблицы хотим достать</param>
    // /// <param name="grantedUserId">id запрашивающего таблицы</param>
    // Task<List<string>?> GetUserSpreadSheetsAsync(long ownerId, long grantedUserId);

    /// <summary>
    /// Выдать права пользавателю к информации другого пользователя
    /// </summary>
    /// <param name="ownerId">id того к данным кого даем доступ</param>
    /// <param name="grantedUserId">id того, кто эти права должен получить</param>
    Task GrantedAccessToUserInfoAsync(long ownerId, long grantedUserId);

    /// <summary>
    /// Забрать права пользавателя к информации другого пользователя
    /// </summary>
    /// <param name="ownerId">id того к данным кого был доступ</param>
    /// <param name="grantedUserId">id того, у кого эти права забрать</param>
    Task DeleteAccessToUserInfoAsync(long ownerId, long grantedUserId);

    /// <summary>
    /// Получить список доступных тренеров
    /// </summary>
    Task<List<User>> GetPublicCoachsAsync();

    /// <summary>
    /// Получить список тренеруемых пользователей
    /// </summary>
    /// <param name="coachId">id тренера</param>
    Task<List<User>> GetCoachStudentsAsync(long coachId);
}