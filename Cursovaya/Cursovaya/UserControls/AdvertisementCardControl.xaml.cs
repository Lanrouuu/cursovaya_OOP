using Cursovaya.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cursovaya.UserControls;

public partial class AdvertisementCardControl : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(AdvertisementCardControl),
            new PropertyMetadata("Без названия"),
            value => value is string text && !string.IsNullOrWhiteSpace(text));

    public static readonly DependencyProperty PriceProperty =
        DependencyProperty.Register(
            nameof(Price),
            typeof(decimal),
            typeof(AdvertisementCardControl),
            new FrameworkPropertyMetadata(0m, null, CoercePriceValue),
            value => value is decimal);

    public static readonly DependencyProperty ImagePathProperty =
        DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(AdvertisementCardControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ShortDescriptionProperty =
        DependencyProperty.Register(nameof(ShortDescription), typeof(string), typeof(AdvertisementCardControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CityProperty =
        DependencyProperty.Register(nameof(City), typeof(string), typeof(AdvertisementCardControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CategoryNameProperty =
        DependencyProperty.Register(nameof(CategoryName), typeof(string), typeof(AdvertisementCardControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SellerNameProperty =
        DependencyProperty.Register(nameof(SellerName), typeof(string), typeof(AdvertisementCardControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CreatedAtProperty =
        DependencyProperty.Register(nameof(CreatedAt), typeof(DateTime), typeof(AdvertisementCardControl), new PropertyMetadata(DateTime.Now));

    public static readonly DependencyProperty IsHighlightedProperty =
        DependencyProperty.Register(nameof(IsHighlighted), typeof(bool), typeof(AdvertisementCardControl), new PropertyMetadata(false));

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(AdvertisementStatus), typeof(AdvertisementCardControl), new PropertyMetadata(AdvertisementStatus.Active));

    public static readonly DependencyProperty ViewCountProperty =
        DependencyProperty.Register(nameof(ViewCount), typeof(int), typeof(AdvertisementCardControl), new PropertyMetadata(0));

    public static readonly DependencyProperty FavoritesCountProperty =
        DependencyProperty.Register(nameof(FavoritesCount), typeof(int), typeof(AdvertisementCardControl), new PropertyMetadata(0));

    public static readonly DependencyProperty IsFavoriteProperty =
        DependencyProperty.Register(nameof(IsFavorite), typeof(bool), typeof(AdvertisementCardControl), new PropertyMetadata(false));

    public static readonly DependencyProperty ToggleFavoriteCommandProperty =
        DependencyProperty.Register(nameof(ToggleFavoriteCommand), typeof(ICommand), typeof(AdvertisementCardControl), new PropertyMetadata(null));

    public static readonly DependencyProperty OpenCommandProperty =
        DependencyProperty.Register(nameof(OpenCommand), typeof(ICommand), typeof(AdvertisementCardControl), new PropertyMetadata(null));

    public static readonly DependencyProperty OpenCommandParameterProperty =
        DependencyProperty.Register(nameof(OpenCommandParameter), typeof(object), typeof(AdvertisementCardControl), new PropertyMetadata(null));

    public static readonly DependencyProperty EditCommandProperty =
        DependencyProperty.Register(nameof(EditCommand), typeof(ICommand), typeof(AdvertisementCardControl), new PropertyMetadata(null));

    public static readonly DependencyProperty EditCommandParameterProperty =
        DependencyProperty.Register(nameof(EditCommandParameter), typeof(object), typeof(AdvertisementCardControl), new PropertyMetadata(null));

    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(AdvertisementCardControl), new PropertyMetadata(null));

    public static readonly DependencyProperty DeleteCommandParameterProperty =
        DependencyProperty.Register(nameof(DeleteCommandParameter), typeof(object), typeof(AdvertisementCardControl), new PropertyMetadata(null));

    public static readonly RoutedEvent AdvertisementSelectedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(AdvertisementSelected),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(AdvertisementCardControl));

    public AdvertisementCardControl()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public decimal Price
    {
        get => (decimal)GetValue(PriceProperty);
        set => SetValue(PriceProperty, value);
    }

    public string ImagePath
    {
        get => (string)GetValue(ImagePathProperty);
        set => SetValue(ImagePathProperty, value);
    }

    public string ShortDescription
    {
        get => (string)GetValue(ShortDescriptionProperty);
        set => SetValue(ShortDescriptionProperty, value);
    }

    public string City
    {
        get => (string)GetValue(CityProperty);
        set => SetValue(CityProperty, value);
    }

    public string CategoryName
    {
        get => (string)GetValue(CategoryNameProperty);
        set => SetValue(CategoryNameProperty, value);
    }

    public string SellerName
    {
        get => (string)GetValue(SellerNameProperty);
        set => SetValue(SellerNameProperty, value);
    }

    public DateTime CreatedAt
    {
        get => (DateTime)GetValue(CreatedAtProperty);
        set => SetValue(CreatedAtProperty, value);
    }

    public bool IsHighlighted
    {
        get => (bool)GetValue(IsHighlightedProperty);
        set => SetValue(IsHighlightedProperty, value);
    }

    public AdvertisementStatus Status
    {
        get => (AdvertisementStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public int ViewCount
    {
        get => (int)GetValue(ViewCountProperty);
        set => SetValue(ViewCountProperty, value);
    }

    public int FavoritesCount
    {
        get => (int)GetValue(FavoritesCountProperty);
        set => SetValue(FavoritesCountProperty, value);
    }

    public bool IsFavorite
    {
        get => (bool)GetValue(IsFavoriteProperty);
        set => SetValue(IsFavoriteProperty, value);
    }

    public ICommand? ToggleFavoriteCommand
    {
        get => (ICommand?)GetValue(ToggleFavoriteCommandProperty);
        set => SetValue(ToggleFavoriteCommandProperty, value);
    }

    public ICommand? OpenCommand
    {
        get => (ICommand?)GetValue(OpenCommandProperty);
        set => SetValue(OpenCommandProperty, value);
    }

    public object? OpenCommandParameter
    {
        get => GetValue(OpenCommandParameterProperty);
        set => SetValue(OpenCommandParameterProperty, value);
    }

    public ICommand? EditCommand
    {
        get => (ICommand?)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public object? EditCommandParameter
    {
        get => GetValue(EditCommandParameterProperty);
        set => SetValue(EditCommandParameterProperty, value);
    }

    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public object? DeleteCommandParameter
    {
        get => GetValue(DeleteCommandParameterProperty);
        set => SetValue(DeleteCommandParameterProperty, value);
    }

    public event RoutedEventHandler AdvertisementSelected
    {
        add => AddHandler(AdvertisementSelectedEvent, value);
        remove => RemoveHandler(AdvertisementSelectedEvent, value);
    }

    private static object CoercePriceValue(DependencyObject dependencyObject, object baseValue)
    {
        var price = (decimal)baseValue;
        return price < 0 ? 0m : price;
    }

    private void OpenAdvertisementCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(AdvertisementSelectedEvent, this));

        if (OpenCommand?.CanExecute(OpenCommandParameter) == true)
        {
            OpenCommand.Execute(OpenCommandParameter);
        }
    }
}
