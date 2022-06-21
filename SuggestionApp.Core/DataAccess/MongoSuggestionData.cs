namespace SuggestionApp.Core.DataAccess;
public class MongoSuggestionData : ISuggestionData
{
   private readonly IDbConnection _db;
   private readonly IUserData _userData;
   private readonly IMemoryCache _cache;
   private readonly IMongoCollection<SuggestionModel> _suggestions;
   private const string CacheName = "SuggestionData";

   public MongoSuggestionData(IDbConnection db, IUserData userData, IMemoryCache cache)
   {
      _db = db;
      _userData = userData;
      _cache = cache;
      _suggestions = db.SuggestionCollection;
   }

   public async Task<List<SuggestionModel>> GetAllSuggestions()
   {
      var output = _cache.Get<List<SuggestionModel>>(CacheName);

      if (output is null)
      {
         var results = await _suggestions.FindAsync(_ => _.Archived == false);
         output = results.ToList();

         _ = _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
      }

      return output;
   }

   public async Task<List<SuggestionModel>> GetAllApprovedSuggestions()
   {
      var output = await GetAllSuggestions();
      return output.Where(_ => _.ApprovedForRelease).ToList();
   }

   public async Task<SuggestionModel> GetSuggestion(string id)
   {
      var results = await _suggestions.FindAsync(_ => _.Id == id);
      return results.FirstOrDefault();
   }

   public async Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApproval()
   {
      var output = await GetAllSuggestions();
      return output.Where(_ =>
         _.ApprovedForRelease == false &&
         _.Rejected == false
      ).ToList();
   }

   public async Task UpdateSuggestion(SuggestionModel suggestion)
   {
      _ = await _suggestions.ReplaceOneAsync(_ => _.Id == suggestion.Id, suggestion);

      _cache.Remove(CacheName);
   }

   public async Task UpvoteSuggestion(string suggestionId, string userId)
   {
      var client = _db.Client;
      using var session = await client.StartSessionAsync();
      session.StartTransaction();

      try
      {
         var db = client.GetDatabase(_db.DbName);
         var suggestionsInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
         var suggestion = (await suggestionsInTransaction.FindAsync(_ => _.Id == suggestionId)).First();

         var isUpvote = suggestion.UserVotes.Add(userId);

         if (!isUpvote)
         {
            suggestion.UserVotes.Remove(userId);
         }

         await suggestionsInTransaction.ReplaceOneAsync(_ => _.Id == suggestionId, suggestion);

         var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
         var user = await _userData.GetUser(suggestion.Author.Id);

         if (isUpvote)
         {
            user.VotedOnSuggestions.Add(new(suggestion));
         }
         else
         {
            var suggestionToRemove = user.VotedOnSuggestions.Where(_ => _.Id == suggestionId).First();
            user.VotedOnSuggestions.Remove(suggestionToRemove);
         }

         await usersInTransaction.ReplaceOneAsync(_ => _.Id == userId, user);

         await session.CommitTransactionAsync();

         _cache.Remove(CacheName);
      }
      catch (Exception)
      {
         await session.AbortTransactionAsync();
         throw;
      }
   }

   public async Task CreateSuggestion(SuggestionModel suggestion)
   {
      var client = _db.Client;
      using var session = await client.StartSessionAsync();
      session.StartTransaction();

      try
      {
         var db = client.GetDatabase(_db.DbName);
         var suggestionsInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
         await suggestionsInTransaction.InsertOneAsync(suggestion);

         var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
         var user = await _userData.GetUser(suggestion.Author.Id);

         user.AuthoredSuggestions.Add(new(suggestion));
         await usersInTransaction.ReplaceOneAsync(_ => _.Id == user.Id, user);

         await session.CommitTransactionAsync();
      }
      catch (Exception ex)
      {
         await session.AbortTransactionAsync();
         throw;
      }
   }
}