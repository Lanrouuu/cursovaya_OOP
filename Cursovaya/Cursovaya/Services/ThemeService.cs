using System.Windows;

namespace Cursovaya.Services;

public class ThemeService
{
    public string CurrentTheme { get; private set; } = "Light";
    public IReadOnlyList<string> Themes { get; } = new[] { "Light", "Dark", "Optimistic", "Blue" };

    public void ApplyTheme(string themeName)
    {
        if (!Themes.Contains(themeName))
        {
            themeName = "Light";
        }

        ReplaceDictionary("Resources/Themes/", $"/Resources/Themes/{themeName}Theme.xaml");
        CurrentTheme = themeName;
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
