namespace DotBoil.Studio.Core.Components.DataTable;

public class BadgeCondition
{
    /// <summary>Value to match from row data. Empty string acts as the default/fallback condition.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>MudBlazor Color enum name: Default, Primary, Secondary, Info, Success, Warning, Error, Dark</summary>
    public string Color { get; set; } = "Default";
}