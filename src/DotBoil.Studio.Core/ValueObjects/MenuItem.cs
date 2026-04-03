namespace DotBoil.Studio.Core.ValueObjects;

public class MenuItem
{
    public string Name { get; set; }
    public string Icon { get; set; }
    public string Path { get; set; }
    public string Header { get; set; }
    public int Rank { get; set; }
    public MenuItem[] Childs { get; set; } = Array.Empty<MenuItem>();
}