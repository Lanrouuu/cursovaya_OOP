using System.Windows;
using System.Windows.Controls;

namespace Cursovaya.UserControls;

public partial class ContactInfoControl : UserControl
{
    public static readonly DependencyProperty SellerNameProperty =
        DependencyProperty.Register(nameof(SellerName), typeof(string), typeof(ContactInfoControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty EmailProperty =
        DependencyProperty.Register(nameof(Email), typeof(string), typeof(ContactInfoControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty PhoneNumberProperty =
        DependencyProperty.Register(nameof(PhoneNumber), typeof(string), typeof(ContactInfoControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ShowActionButtonProperty =
        DependencyProperty.Register(nameof(ShowActionButton), typeof(bool), typeof(ContactInfoControl), new PropertyMetadata(true));

    public static readonly RoutedEvent ContactClickedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(ContactClicked),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ContactInfoControl));

    public ContactInfoControl()
    {
        InitializeComponent();
    }

    public string SellerName
    {
        get => (string)GetValue(SellerNameProperty);
        set => SetValue(SellerNameProperty, value);
    }

    public string Email
    {
        get => (string)GetValue(EmailProperty);
        set => SetValue(EmailProperty, value);
    }

    public string PhoneNumber
    {
        get => (string)GetValue(PhoneNumberProperty);
        set => SetValue(PhoneNumberProperty, value);
    }

    public bool ShowActionButton
    {
        get => (bool)GetValue(ShowActionButtonProperty);
        set => SetValue(ShowActionButtonProperty, value);
    }

    public event RoutedEventHandler ContactClicked
    {
        add => AddHandler(ContactClickedEvent, value);
        remove => RemoveHandler(ContactClickedEvent, value);
    }

    private void ShowContactsCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ContactClickedEvent, this));
    }
}
