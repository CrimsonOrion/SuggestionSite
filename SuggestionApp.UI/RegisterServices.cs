namespace SuggestionApp.UI;

public static class RegisterServices
{
   public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
   {
      // Add services to the container.
      builder.Services.AddRazorPages();
      builder.Services.AddServerSideBlazor();
      builder.Services.AddMemoryCache();

      return builder;
   }
}