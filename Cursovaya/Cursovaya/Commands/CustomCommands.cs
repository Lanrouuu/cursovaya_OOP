using System.Windows.Input;

namespace Cursovaya.Commands;

public static class CustomCommands
{
    public static readonly RoutedUICommand ShowContactsCommand = new(
        nameof(ShowContactsCommand),
        nameof(ShowContactsCommand),
        typeof(CustomCommands));

    public static readonly RoutedUICommand OpenAdvertisementCommand = new(
        nameof(OpenAdvertisementCommand),
        nameof(OpenAdvertisementCommand),
        typeof(CustomCommands));
}
