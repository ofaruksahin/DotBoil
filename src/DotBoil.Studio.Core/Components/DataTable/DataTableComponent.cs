using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Components.Form;
using DotBoil.Studio.Core.Contracts;
using MudBlazor;

namespace DotBoil.Studio.Core.Components.DataTable;

public class DataTableComponent : BaseComponent
{
    public override string Category => "Data";
    public override string Title => "Data Table";
    public override string ComponentIcon => Icons.Material.Rounded.TableChart;
    public override Type RendererType => typeof(DataTableComponentRenderer);

    [FieldProperty(1, "Data URL", "The endpoint URL to fetch table data from.", typeof(ApiUrlSettingsRenderer))]
    public string DataUrl { get; set; } = string.Empty;

    [FieldProperty(2, "Paginated Table", "When enabled, data is fetched with PaginationFilter query parameters and the response is expected as PaginatedModel.")]
    public bool IsPaginatedTable { get; set; } = false;

    [FieldProperty(3, "Page Size", "Number of records per page. Only used when pagination is enabled.")]
    public int PageSize { get; set; } = 10;

    [FieldProperty(4, "Show Create Button", "When enabled, a create button is displayed above the table.")]
    public bool ShowCreateButton { get; set; } = false;

    [FieldProperty(5, "Create Button Text", "Text displayed on the create button.")]
    public string CreateButtonText { get; set; } = "Yeni Ekle";

    [FieldProperty(6, "Create Button Icon", "Icon displayed on the create button.", typeof(IconSelectionSettingsRenderer))]
    public Icon CreateButtonIcon { get; set; } = new();

    [FieldProperty(7, "Create Button URL", "URL to navigate when the create button is clicked.")]
    public string CreateButtonUrl { get; set; } = string.Empty;

    [FieldProperty(8, "No Data Text", "Message displayed when the table has no records.")]
    public string NoDataText { get; set; } = "Kayıt bulunamadı.";

    [FieldProperty(9, "Columns", "Define the columns to display in the table.", typeof(DataTableColumnsRenderer))]
    public List<DataTableColumn> Columns { get; set; } = new();
}