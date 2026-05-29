using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodieApp.Models;
using FoodieApp.Services;

namespace FoodieApp.ViewModels;

/// <summary>View model for the favourites list page.</summary>
public partial class FavouritesViewModel : BaseViewModel
{
    private readonly FavouritesService _favouritesService;
    private readonly RecipeService _recipeService;

    public ObservableCollection<Recipe> Favourites { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(HasFavourites))]
    private bool _isLoaded;

    public bool IsEmpty => IsLoaded && Favourites.Count == 0;
    public bool HasFavourites => Favourites.Count > 0;

    public FavouritesViewModel(FavouritesService favouritesService, RecipeService recipeService)
    {
        _favouritesService = favouritesService;
        _recipeService = recipeService;
        Title = "Favourites";
    }

    /// <summary>Loads all favourited recipes from storage and the API.</summary>
    [RelayCommand]
    public async Task LoadFavouritesAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            Favourites.Clear();
            var ids = _favouritesService.GetFavouriteIds();

            foreach (var id in ids)
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(id);
                if (recipe != null)
                {
                    recipe.IsFavourite = true;
                    Favourites.Add(recipe);
                }
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
            IsLoaded = true;
        }
    }

    /// <summary>Removes a recipe from the favourites list.</summary>
    [RelayCommand]
    public void RemoveFavourite(Recipe recipe)
    {
        if (recipe is null) return;

        _favouritesService.RemoveFavourite(recipe.Id);
        Favourites.Remove(recipe);
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasFavourites));
        HapticFeedback.Perform(HapticFeedbackType.Click);
    }

    /// <summary>Navigates to the recipe detail page for the selected favourite.</summary>
    [RelayCommand]
    public async Task OpenRecipeAsync(Recipe recipe)
    {
        if (recipe is null) return;
        await Shell.Current.GoToAsync($"RecipeDetailPage?recipeId={recipe.Id}");
    }
}
