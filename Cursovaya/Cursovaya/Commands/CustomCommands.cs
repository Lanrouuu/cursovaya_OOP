using System.Windows.Input;

namespace Cursovaya.Commands;

public static class CustomCommands
{
    public static readonly RoutedUICommand ShowContactsCommand = new(
        "Показать контакты",
        nameof(ShowContactsCommand),
        typeof(CustomCommands));

    public static readonly RoutedUICommand OpenAdvertisementCommand = new(
        "Открыть объявление",
        nameof(OpenAdvertisementCommand),
        typeof(CustomCommands));
}
