using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodieApp.Models;
using FoodieApp.Services;
namespace FoodieApp.ViewModels;
public partial class RecipesViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;
    private readonly FavouritesService _favouritesService;
    private List<Recipe> _allRecipes = new();
    public ObservableCollection<Recipe> Recipes { get; } = new();
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _searchValidationMessage = string.Empty;
    [ObservableProperty] private bool _isValidationVisible;
    public RecipesViewModel(RecipeService recipeService, FavouritesService favouritesService)
    {
        _recipeService = recipeService; _favouritesService = favouritesService; Title = "Foodie";
    }
    [RelayCommand]
    public async Task LoadRecipesAsync()
    {
        if (IsBusy) return; IsBusy = true;
        try { UpdateRecipeList(await _recipeService.SearchRecipesAsync("a")); }
        finally { IsBusy = false; }
    }
    [RelayCommand]
    public async Task SearchAsync()
    {
        if (!string.IsNullOrWhiteSpace(SearchText) && SearchText.Trim().Length < 2)
        { SearchValidationMessage = "Please enter at least 2 characters."; IsValidationVisible = true; return; }
        IsValidationVisible = false;
        if (IsBusy) return; IsBusy = true;
        try
        {
            var keyword = string.IsNullOrWhiteSpace(SearchText) ? "a" : SearchText.Trim();
            var results = await _recipeService.SearchRecipesAsync(keyword);
            UpdateRecipeList(results);
            if (results.Count == 0) { SearchValidationMessage = "No recipes found. Try a different keyword."; IsValidationVisible = true; }
        }
        finally { IsBusy = false; }
    }
    public Recipe? GetRandomRecipe()
    {
        if (_allRecipes.Count == 0) return null;
        return _allRecipes[new Random().Next(_allRecipes.Count)];
    }
    private void UpdateRecipeList(List<Recipe> recipes)
    {
        _allRecipes = recipes;
        var favIds = _favouritesService.GetFavouriteIds();
        foreach (var r in _allRecipes) r.IsFavourite = favIds.Contains(r.Id);
        Recipes.Clear();
        foreach (var r in _allRecipes) Recipes.Add(r);
    }
}