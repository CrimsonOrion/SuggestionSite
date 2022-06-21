namespace SuggestionApp.Core.DataAccess;

public interface ISuggestionData
{
   Task CreateSuggestion(SuggestionModel suggestion);
   Task<List<SuggestionModel>> GetAllApprovedSuggestions();
   Task<List<SuggestionModel>> GetAllSuggestions();
   Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApproval();
   Task<SuggestionModel> GetSuggestion(string id);
   Task UpdateSuggestion(SuggestionModel suggestion);
   Task UpvoteSuggestion(string suggestionId, string userId);
}