using Cursovaya.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Cursovaya.Views;

public partial class AddEditAdvertisementView : UserControl
{
    public AddEditAdvertisementView()
    {
        InitializeComponent();
    }

    private void ImageDropBorder_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void ImageDropBorder_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        var files = e.Data.GetData(DataFormats.FileDrop) as string[];
        var file = files?.FirstOrDefault();
        if (file == null) return;

        if (DataContext is AddEditAdvertisementViewModel vm)
            vm.ApplyDroppedImage(file);
    }
}
