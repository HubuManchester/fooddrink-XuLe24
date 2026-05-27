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
    { _recipeService = recipeService; _favouritesService = favouritesService; }
    [RelayCommand]
    public async Task LoadRecipeAsync()
    {
        if (string.IsNullOrEmpty(RecipeId) || IsBusy) return; IsBusy = true;
        try
        {
            Recipe = await _recipeService.GetRecipeByIdAsync(RecipeId);
            if (Recipe != null) { Title = Recipe.Name; IsFavourite = _favouritesService.IsFavourite(Recipe.Id); CurrentStepIndex = 0; }
        }
        finally { IsBusy = false; }
    }
    [RelayCommand(CanExecute = nameof(CanGoBack))]
    public void PreviousStep() { if (CanGoBack) CurrentStepIndex--; }
    [RelayCommand(CanExecute = nameof(CanGoForward))]
    public void NextStep() { if (CanGoForward) CurrentStepIndex++; }
    [RelayCommand]
    public async Task SpeakCurrentStepAsync()
    {
        if (string.IsNullOrEmpty(CurrentStep)) return;
        _ttsCts?.Cancel(); _ttsCts = new CancellationTokenSource(); IsSpeaking = true;
        try { await TextToSpeech.SpeakAsync(CurrentStep, new SpeechOptions { Pitch = 1.0f, Volume = 1.0f }, _ttsCts.Token); }
        catch (OperationCanceledException) { }
        finally { IsSpeaking = false; }
    }
    [RelayCommand]
    public void StopSpeaking() { _ttsCts?.Cancel(); IsSpeaking = false; }
    [RelayCommand]
    public void ToggleFavourite()
    {
        if (Recipe == null) return;
        IsFavourite = _favouritesService.Toggle(Recipe.Id);
        Recipe.IsFavourite = IsFavourite;
        HapticFeedback.Perform(IsFavourite ? HapticFeedbackType.LongPress : HapticFeedbackType.Click);
    }
}