using Microsoft.Win32;
using System.Windows;

namespace Cursovaya.Services;

public class DialogService
{
    public void ShowMessage(string message, string? title = null)
    {
        MessageBox.Show(message, title ?? LocalizedStrings.Get("DialogInfoTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message)
    {
        MessageBox.Show(message, LocalizedStrings.Get("DialogErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public bool Confirm(string message)
    {
        return MessageBox.Show(message, LocalizedStrings.Get("DialogConfirmTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public string? SelectImagePath()
    {
        var dialog = new OpenFileDialog
        {
            Title = LocalizedStrings.Get("SelectImageDialogTitle"),
            Filter = LocalizedStrings.Get("ImageDialogFilter")
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
