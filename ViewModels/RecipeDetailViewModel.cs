using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodieApp.Models;
using FoodieApp.Services;

namespace FoodieApp.ViewModels;

[QueryProperty(nameof(RecipeId), "recipeId")]
public partial class RecipeDetailViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;
    private readonly FavouritesService _favouritesService;
    private CancellationTokenSource? _ttsCts;

    [ObservableProperty] private string _recipeId = string.Empty;
    [ObservableProperty] private Recipe? _recipe;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentStep))]
    [NotifyPropertyChangedFor(nameof(StepProgress))]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyPropertyChangedFor(nameof(CanGoForward))]
    private int _currentStepIndex;
    [ObservableProperty] private bool _isSpeaking;
    [ObservableProperty] private bool _isFavourite;

    public string CurrentStep  => Recipe?.Steps.ElementAtOrDefault(CurrentStepIndex) ?? string.Empty;
    public string StepProgress => Recipe != null ? $"Step {CurrentStepIndex + 1} of {Recipe.Steps.Count}" : string.Empty;
    public bool CanGoBack    => CurrentStepIndex > 0;
    public bool CanGoForward => Recipe != null && CurrentStepIndex < Recipe.Steps.Count - 1;

    public RecipeDetailViewModel(RecipeService recipeService, FavouritesService favouritesService)
    {
        _recipeService = recipeService;
        _favouritesService = favouritesService;
    }

    [RelayCommand]
    public async Task LoadRecipeAsync()
    {
        if (string.IsNullOrEmpty(RecipeId) || IsBusy) return;
        IsBusy = true;
        try
        {
            Recipe = await _recipeService.GetRecipeByIdAsync(RecipeId);
            if (Recipe != null)
            {
                Title = Recipe.Name;
                IsFavourite = _favouritesService.IsFavourite(Recipe.Id);
                CurrentStepIndex = 0;
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    public void PreviousStep()
    {
        if (CanGoBack) CurrentStepIndex--;
    }

    [RelayCommand(CanExecute = nameof(CanGoForward))]
    public void NextStep()
    {
        if (CanGoForward) CurrentStepIndex++;
    }

    [RelayCommand]
    public async Task ReadAloudAsync()
    {
        if (Recipe is null) return;

        var ingredients = Recipe.Ingredients.Count > 0
            ? string.Join(", ", Recipe.Ingredients)
            : "No ingredients listed";
        var text = $"{Recipe.Name}. Ingredients: {ingredients}";

        _ttsCts?.Cancel();
        _ttsCts?.Dispose();
        _ttsCts = new CancellationTokenSource();
        IsSpeaking = true;

        try
        {
            var options = await TtsHelper.GetEnglishSpeechOptionsAsync();
            await TextToSpeech.Default.SpeakAsync(text, options, _ttsCts.Token);
        }
        catch (OperationCanceledException) { }
        finally { IsSpeaking = false; }
    }

    [RelayCommand]
    public Task StopReadingAsync()
    {
        if (_ttsCts is { IsCancellationRequested: false })
            _ttsCts.Cancel();

        IsSpeaking = false;
        return Task.CompletedTask;
    }

    [RelayCommand]
    public void ToggleFavourite()
    {
        if (Recipe is null) return;
        IsFavourite = _favouritesService.Toggle(Recipe.Id);
        Recipe.IsFavourite = IsFavourite;
        HapticFeedback.Perform(IsFavourite ? HapticFeedbackType.LongPress : HapticFeedbackType.Click);
    }
}
