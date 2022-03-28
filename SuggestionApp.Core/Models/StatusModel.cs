namespace SuggestionApp.Core.Models;
public class StatusModel
{
   [BsonId] // <- Identity
   [BsonRepresentation(BsonType.ObjectId)]
   public string Id { get; set; }
   public string StatusName { get; set; }
   public string StatusDescription { get; set; }
}