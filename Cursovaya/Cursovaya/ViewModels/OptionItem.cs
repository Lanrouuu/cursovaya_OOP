using System.Windows;

namespace Cursovaya.ViewModels;

public class OptionItem
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ResourceKey { get; set; } = string.Empty;

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(ResourceKey)
            ? Title
            : Application.Current.TryFindResource(ResourceKey) as string ?? Title;
    }
}
