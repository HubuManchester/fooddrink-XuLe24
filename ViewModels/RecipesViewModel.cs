using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodieApp.Models;
using FoodieApp.Services;

namespace FoodieApp.ViewModels;

/// <summary>View model for the home page recipe list and search.</summary>
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
        _recipeService = recipeService;
        _favouritesService = favouritesService;
        Title = "Foodie";
    }

    /// <summary>Loads the default recipe list from the API.</summary>
    [RelayCommand]
    public async Task LoadRecipesAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            UpdateRecipeList(await _recipeService.SearchRecipesAsync("a"));
        }
        catch (HttpRequestException)
        {
            if (ErrorHelper.GetCurrentPage() is Page page)
                await ErrorHelper.ShowNetworkErrorAsync(page);
        }
        catch (Exception ex)
        {
            if (ErrorHelper.GetCurrentPage() is Page page)
                await ErrorHelper.ShowUnexpectedErrorAsync(page, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Searches recipes using the current search text.</summary>
    [RelayCommand]
    public async Task SearchAsync()
    {
        if (!string.IsNullOrWhiteSpace(SearchText) && SearchText.Trim().Length < 2)
        {
            SearchValidationMessage = "Please enter at least 2 characters.";
            IsValidationVisible = true;
            return;
        }

        IsValidationVisible = false;
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var keyword = string.IsNullOrWhiteSpace(SearchText) ? "a" : SearchText.Trim();
            var results = await _recipeService.SearchRecipesAsync(keyword);
            UpdateRecipeList(results);

            if (results.Count == 0)
            {
                SearchValidationMessage = "No recipes found. Try a different keyword.";
                IsValidationVisible = true;
            }
        }
        catch (HttpRequestException)
        {
            if (ErrorHelper.GetCurrentPage() is Page page)
                await ErrorHelper.ShowNetworkErrorAsync(page);
        }
        catch (Exception ex)
        {
            if (ErrorHelper.GetCurrentPage() is Page page)
                await ErrorHelper.ShowUnexpectedErrorAsync(page, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Returns a random recipe from the currently loaded list.</summary>
    public Recipe? GetRandomRecipe()
    {
        if (_allRecipes.Count == 0) return null;
        return _allRecipes[Random.Shared.Next(_allRecipes.Count)];
    }

    private void UpdateRecipeList(List<Recipe> recipes)
    {
        _allRecipes = recipes;
        var favouriteIds = _favouritesService.GetFavouriteIds();

        foreach (var recipe in _allRecipes)
            recipe.IsFavourite = favouriteIds.Contains(recipe.Id);

        Recipes.Clear();
        foreach (var recipe in _allRecipes)
            Recipes.Add(recipe);
    }
}
