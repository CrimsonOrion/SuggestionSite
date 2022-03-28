namespace SuggestionApp.Core.Models;
public class BasicUserModel
{
   [BsonRepresentation(BsonType.ObjectId)]
   public string Id { get; set; }
   public string DisplayName { get; set; }

   public BasicUserModel() { }

   public BasicUserModel(UserModel model)
   {
      Id = model.Id;
      DisplayName = model.DisplayName;
   }
}