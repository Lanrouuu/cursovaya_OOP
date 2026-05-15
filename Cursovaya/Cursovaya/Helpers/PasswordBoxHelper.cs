using System.Windows;
using System.Windows.Controls;

namespace Cursovaya.Helpers;

public static class PasswordBoxHelper
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxHelper),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

    private static readonly DependencyProperty UpdatingPasswordProperty =
        DependencyProperty.RegisterAttached(
            "UpdatingPassword",
            typeof(bool),
            typeof(PasswordBoxHelper));

    public static readonly DependencyProperty IsPasswordEmptyProperty =
        DependencyProperty.RegisterAttached(
            "IsPasswordEmpty",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(true));

    public static string GetBoundPassword(DependencyObject d) => d.GetValue(BoundPasswordProperty) as string ?? string.Empty;
    public static void SetBoundPassword(DependencyObject d, string value) => d.SetValue(BoundPasswordProperty, value ?? string.Empty);
    public static bool GetIsPasswordEmpty(DependencyObject d) => (bool)d.GetValue(IsPasswordEmptyProperty);
    public static void SetIsPasswordEmpty(DependencyObject d, bool value) => d.SetValue(IsPasswordEmptyProperty, value);

    private static bool GetUpdatingPassword(DependencyObject d) => (bool)d.GetValue(UpdatingPasswordProperty);
    private static void SetUpdatingPassword(DependencyObject d, bool value) => d.SetValue(UpdatingPasswordProperty, value);

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox box)
            return;

        box.PasswordChanged -= HandlePasswordChanged;

        if (!GetUpdatingPassword(box))
        {
            box.Password = e.NewValue as string ?? string.Empty;
        }

        SetIsPasswordEmpty(box, string.IsNullOrEmpty(box.Password));
        box.PasswordChanged += HandlePasswordChanged;
    }

    private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox box)
            return;

        SetUpdatingPassword(box, true);
        SetBoundPassword(box, box.Password);
        SetIsPasswordEmpty(box, string.IsNullOrEmpty(box.Password));
        SetUpdatingPassword(box, false);
    }
}
