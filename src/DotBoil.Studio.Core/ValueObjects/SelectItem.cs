namespace DotBoil.Studio.Core.ValueObjects;

public class SelectItem
{
    public string Text { get; set; }
    public string Value { get; set; }
    public bool IsSelected { get; set; }

    public SelectItem()
    {
    }

    public SelectItem(string text, string value, bool isSelected)
    {
        Text = text;
        Value = value;
        IsSelected = isSelected;
    }
}