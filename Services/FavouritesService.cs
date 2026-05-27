using System.Text.Json;
namespace FoodieApp.Services;
public class FavouritesService
{
    private const string PrefsKey = "favourite_ids";
    public HashSet<string> GetFavouriteIds()
    {
        var json = Preferences.Get(PrefsKey, string.Empty);
        if (string.IsNullOrEmpty(json)) return new HashSet<string>();
        try { var list = JsonSerializer.Deserialize<List<string>>(json); return list != null ? new HashSet<string>(list) : new HashSet<string>(); }
        catch (JsonException) { return new HashSet<string>(); }
    }
    public bool IsFavourite(string recipeId) => GetFavouriteIds().Contains(recipeId);
    public void AddFavourite(string recipeId) { var ids = GetFavouriteIds(); ids.Add(recipeId); Save(ids); }
    public void RemoveFavourite(string recipeId) { var ids = GetFavouriteIds(); ids.Remove(recipeId); Save(ids); }
    public bool Toggle(string recipeId)
    {
        if (IsFavourite(recipeId)) { RemoveFavourite(recipeId); return false; }
        AddFavourite(recipeId); return true;
    }
    private static void Save(HashSet<string> ids) => Preferences.Set(PrefsKey, JsonSerializer.Serialize(ids.ToList()));
}