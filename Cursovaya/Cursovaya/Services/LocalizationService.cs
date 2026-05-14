using System.Windows;
using System.Globalization;

namespace Cursovaya.Services;

public class LocalizationService
{
    public string CurrentLanguage { get; private set; } = "ru-RU";
    public IReadOnlyList<string> Languages { get; } = new[] { "ru-RU", "en-US" };

    public void ApplyLanguage(string language)
    {
        if (!Languages.Contains(language))
        {
            language = "ru-RU";
        }

        ReplaceDictionary("Resources/Languages/", $"/Resources/Languages/Strings.{language}.xaml");
        CurrentLanguage = language;
        CultureInfo.CurrentCulture = new CultureInfo(language);
        CultureInfo.CurrentUICulture = new CultureInfo(language);
    }

    private static void ReplaceDictionary(string marker, string newSource)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var oldDictionaries = dictionaries
            .Where(x => x.Source != null && x.Source.OriginalString.Contains(marker, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var dictionary in oldDictionaries)
        {
            dictionaries.Remove(dictionary);
        }

        dictionaries.Add(new ResourceDictionary { Source = new Uri(newSource, UriKind.Relative) });
    }
}
