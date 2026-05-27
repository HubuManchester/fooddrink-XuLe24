using System.Net.Http.Json;
using FoodieApp.Models;
namespace FoodieApp.Services;
public class RecipeService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.themealdb.com/api/json/v1/1/";
    private List<Recipe>? _cachedRecipes;
    public RecipeService(HttpClient httpClient) { _httpClient = httpClient; }
    public async Task<List<Recipe>> SearchRecipesAsync(string keyword = "a")
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<MealApiResponse>($"{BaseUrl}search.php?s={keyword}");
            if (response?.Meals == null) return _cachedRecipes ?? new List<Recipe>();
            _cachedRecipes = response.Meals.Select(MapToRecipe).ToList();
            return _cachedRecipes;
        }
        catch (HttpRequestException) { if (_cachedRecipes != null) return _cachedRecipes; throw; }
    }
    public async Task<Recipe?> GetRecipeByIdAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<MealApiResponse>($"{BaseUrl}lookup.php?i={id}");
            return response?.Meals?.Select(MapToRecipe).FirstOrDefault();
        }
        catch (HttpRequestException) { return _cachedRecipes?.FirstOrDefault(r => r.Id == id); }
    }
    private static Recipe MapToRecipe(MealDto dto) => new()
    {
        Id = dto.IdMeal, Name = dto.StrMeal, Category = dto.StrCategory,
        Area = dto.StrArea, ImageUrl = dto.StrMealThumb,
        Ingredients = BuildIngredientList(dto),
        Steps = SplitIntoSteps(dto.StrInstructions),
        PrepTimeMinutes = EstimatePrepTime(SplitIntoSteps(dto.StrInstructions).Count),
    };
    private static List<string> BuildIngredientList(MealDto dto)
    {
        var pairs = new (string? i, string? m)[]
        {
            (dto.StrIngredient1,dto.StrMeasure1),(dto.StrIngredient2,dto.StrMeasure2),
            (dto.StrIngredient3,dto.StrMeasure3),(dto.StrIngredient4,dto.StrMeasure4),
            (dto.StrIngredient5,dto.StrMeasure5),(dto.StrIngredient6,dto.StrMeasure6),
            (dto.StrIngredient7,dto.StrMeasure7),(dto.StrIngredient8,dto.StrMeasure8),
            (dto.StrIngredient9,dto.StrMeasure9),(dto.StrIngredient10,dto.StrMeasure10),
            (dto.StrIngredient11,dto.StrMeasure11),(dto.StrIngredient12,dto.StrMeasure12),
            (dto.StrIngredient13,dto.StrMeasure13),(dto.StrIngredient14,dto.StrMeasure14),
            (dto.StrIngredient15,dto.StrMeasure15),
        };
        return pairs.Where(p => !string.IsNullOrWhiteSpace(p.i))
                    .Select(p => $"{p.m?.Trim()} {p.i?.Trim()}".Trim()).ToList();
    }
    private static List<string> SplitIntoSteps(string instructions)
    {
        if (string.IsNullOrWhiteSpace(instructions)) return new List<string> { "No instructions available." };
        return instructions.Split(new[] { "\r\n", "\n", ". " }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(s => s.Trim()).Where(s => s.Length > 10).ToList();
    }
    private static int EstimatePrepTime(int stepCount) => Math.Max(10, stepCount * 5);
}