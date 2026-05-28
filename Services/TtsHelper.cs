namespace FoodieApp.Services;

public static class TtsHelper
{
    private static Locale? _englishLocale;

    public static async Task<SpeechOptions> GetEnglishSpeechOptionsAsync()
    {
        _englishLocale ??= await FindEnglishLocaleAsync();
        return new SpeechOptions
        {
            Pitch = 1.0f,
            Volume = 1.0f,
            Locale = _englishLocale
        };
    }

    private static async Task<Locale?> FindEnglishLocaleAsync()
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();

        return locales.FirstOrDefault(l =>
                   l.Language.Equals("en", StringComparison.OrdinalIgnoreCase) &&
                   l.Country?.Equals("US", StringComparison.OrdinalIgnoreCase) == true)
               ?? locales.FirstOrDefault(l =>
                   l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase));
    }
}
