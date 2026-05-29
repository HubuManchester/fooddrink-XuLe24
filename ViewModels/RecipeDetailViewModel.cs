using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodieApp.Models;
using FoodieApp.Services;

namespace FoodieApp.ViewModels;

/// <summary>View model for the recipe detail page with TTS and step navigation.</summary>
[QueryProperty(nameof(RecipeId), "recipeId")]
public partial class RecipeDetailViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;
    private readonly FavouritesService _favouritesService;
    private CancellationTokenSource? _ttsCts;

    [ObservableProperty] private string _recipeId = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentStep))]
    [NotifyPropertyChangedFor(nameof(StepProgress))]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyPropertyChangedFor(nameof(CanGoForward))]
    [NotifyPropertyChangedFor(nameof(HasSteps))]
    [NotifyPropertyChangedFor(nameof(HasNoSteps))]
    private Recipe? _recipe;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentStep))]
    [NotifyPropertyChangedFor(nameof(StepProgress))]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyPropertyChangedFor(nameof(CanGoForward))]
    [NotifyCanExecuteChangedFor(nameof(PreviousStepCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextStepCommand))]
    private int _currentStepIndex;
    [ObservableProperty] private bool _isSpeaking;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FavouriteButtonText))]
    [NotifyPropertyChangedFor(nameof(FavouriteButtonHint))]
    private bool _isFavourite;

    public string FavouriteButtonText => IsFavourite ? "♥ Saved to Favourites" : "♡ Add to Favourites";
    public string FavouriteButtonHint => IsFavourite
        ? "Remove this recipe from favourites"
        : "Save this recipe to favourites";

    public string CurrentStep => Recipe?.Steps.ElementAtOrDefault(CurrentStepIndex) ?? string.Empty;
    public string StepProgress => Recipe != null
        ? $"Step {CurrentStepIndex + 1} of {Recipe.Steps.Count}"
        : string.Empty;
    public bool CanGoBack => CurrentStepIndex > 0;
    public bool CanGoForward => Recipe != null && Recipe.Steps.Count > 0 && CurrentStepIndex < Recipe.Steps.Count - 1;
    public bool HasSteps => Recipe?.Steps.Count > 0;
    public bool HasNoSteps => Recipe is not null && Recipe.Steps.Count == 0;

    public RecipeDetailViewModel(RecipeService recipeService, FavouritesService favouritesService)
    {
        _recipeService = recipeService;
        _favouritesService = favouritesService;
    }

    /// <summary>Loads the recipe details from the API using the recipe ID.</summary>
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

    /// <summary>Moves to the previous cooking step.</summary>
    [RelayCommand(CanExecute = nameof(CanGoBack))]
    public void PreviousStep()
    {
        if (CanGoBack) CurrentStepIndex--;
    }

    /// <summary>Moves to the next cooking step.</summary>
    [RelayCommand(CanExecute = nameof(CanGoForward))]
    public void NextStep()
    {
        if (CanGoForward) CurrentStepIndex++;
    }

    /// <summary>Reads the recipe name and ingredients aloud in English.</summary>
    [RelayCommand]
    public async Task ReadAloudAsync()
    {
        if (Recipe is null) return;

        var ingredients = Recipe.Ingredients.Count > 0
            ? string.Join(", ", Recipe.Ingredients)
            : "No ingredients listed";

        await SpeakTextAsync($"{Recipe.Name}. Ingredients: {ingredients}");
    }

    /// <summary>Reads the current cooking step aloud in English.</summary>
    [RelayCommand]
    public async Task ReadStepsAsync()
    {
        if (Recipe is null || string.IsNullOrEmpty(CurrentStep)) return;
        await SpeakTextAsync($"{StepProgress}. {CurrentStep}");
    }

    /// <summary>Stops any active text-to-speech playback.</summary>
    [RelayCommand]
    public Task StopReadingAsync()
    {
        if (_ttsCts is { IsCancellationRequested: false })
            _ttsCts.Cancel();

        IsSpeaking = false;
        return Task.CompletedTask;
    }

    /// <summary>Toggles the favourite status of the current recipe.</summary>
    [RelayCommand]
    public void ToggleFavourite()
    {
        if (Recipe is null) return;

        IsFavourite = _favouritesService.Toggle(Recipe.Id);
        Recipe.IsFavourite = IsFavourite;
        HapticFeedback.Perform(IsFavourite ? HapticFeedbackType.LongPress : HapticFeedbackType.Click);
    }

    private async Task SpeakTextAsync(string text)
    {
        _ttsCts?.Cancel();
        _ttsCts?.Dispose();
        _ttsCts = new CancellationTokenSource();
        IsSpeaking = true;

        try
        {
            var options = await TtsHelper.GetEnglishSpeechOptionsAsync();
            var speechText = TtsHelper.PrepareEnglishSpeech(text);
            await TextToSpeech.Default.SpeakAsync(speechText, options, _ttsCts.Token);
        }
        catch (OperationCanceledException) { }
        finally
        {
            IsSpeaking = false;
        }
    }
}
