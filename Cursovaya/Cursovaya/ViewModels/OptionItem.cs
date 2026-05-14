namespace Cursovaya.ViewModels;

public class OptionItem
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public override string ToString()
    {
        return Title;
    }
}
