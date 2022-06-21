namespace SuggestionApp.Core.DataAccess;

public interface IUserData
{
   Task CreateUser(UserModel user);
   Task<List<UserModel>> GetAllUsersAsync();
   Task<UserModel> GetUser(string id);
   Task<UserModel> GetUserFromAuthentication(string objectId);
   Task UpdateUser(UserModel user);
}