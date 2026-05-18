using System.Windows.Controls;
using System.Windows.Input;

namespace Cursovaya.UserControls;

public partial class SearchFilterControl : UserControl
{
    public SearchFilterControl()
    {
        InitializeComponent();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {

    }

    private void DatePicker_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is DatePicker picker && picker.IsDropDownOpen)
        {
            picker.IsDropDownOpen = false;
            e.Handled = true;
        }
    }
}
