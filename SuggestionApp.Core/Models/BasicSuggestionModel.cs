namespace SuggestionApp.Core.Models;
public class BasicSuggestionModel
{
   [BsonRepresentation(BsonType.ObjectId)]
   public string Id { get; set; }
   public string Suggestion { get; set; }

   public BasicSuggestionModel() { }

   public BasicSuggestionModel(SuggestionModel model)
   {
      Id = model.Id;
      Suggestion = model.Suggestion;
   }
}