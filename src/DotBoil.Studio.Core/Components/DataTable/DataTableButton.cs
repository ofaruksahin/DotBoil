namespace DotBoil.Studio.Core.Components.DataTable;

public class DataTableButton
{
    public DataTableButtonType ButtonType { get; set; } = DataTableButtonType.Redirect;

    /// <summary>MudBlazor Color enum name: Default, Primary, Secondary, Info, Success, Warning, Error, Dark</summary>
    public string Color { get; set; } = "Default";

    public string Text { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string Tooltip { get; set; } = string.Empty;

    /// <summary>Used when ButtonType == Redirect. Supports {fieldName} placeholders resolved from row data.</summary>
    public string RedirectUrl { get; set; } = string.Empty;

    /// <summary>Used when ButtonType == Action. HTTP request is sent to this URL.</summary>
    public string ActionUrl { get; set; } = string.Empty;

    /// <summary>Used when ButtonType == Action. HTTP method for the action request (GET, POST, PUT, PATCH, DELETE).</summary>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>Used when ButtonType == Action. Shown in the confirmation dialog before the action is executed.</summary>
    public string ConfirmationText { get; set; } = string.Empty;
}