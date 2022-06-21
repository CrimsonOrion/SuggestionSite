using Microsoft.Extensions.Caching.Memory;

namespace SuggestionApp.Core.DataAccess;
public class MongoCategoryData : ICategoryData
{
   IMongoCollection<CategoryModel> _categories;
   private readonly IMemoryCache _cache;
   private const string CacheName = "CategoryData";

   public MongoCategoryData(IDbConnection db, IMemoryCache cache)
   {
      _cache = cache;
      _categories = db.CategoryCollection;
   }

   public async Task<List<CategoryModel>> GetAllCategories()
   {
      var output = _cache.Get<List<CategoryModel>>(CacheName);

      if (output is null)
      {
         var results = await _categories.FindAsync(_ => true);
         output = results.ToList();

         _cache.Set(CacheName, output, TimeSpan.FromDays(1));
      }

      return output;
   }

   public Task CreateCategory(CategoryModel category)
   {
      return _categories.InsertOneAsync(category);
   }
}