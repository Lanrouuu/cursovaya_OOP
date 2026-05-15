using System.Globalization;
using System.Windows;

namespace Cursovaya.Services;

public static class LocalizedStrings
{
    public static string Get(string key)
    {
        return Application.Current?.TryFindResource(key) as string ?? key;
    }

    public static string Format(string key, params object[] args)
    {
        return string.Format(CultureInfo.CurrentCulture, Get(key), args);
    }
}
