namespace DotBoil.Studio.Core.Components.DataTable;

public class DataTableColumn
{
    public string Field { get; set; } = string.Empty;

    public string Header { get; set; } = string.Empty;

    /// <summary>Optional .NET format string applied to the cell value (e.g. "dd.MM.yyyy", "N2").</summary>
    public string Format { get; set; } = string.Empty;

    public DataTableColumnType ColumnType { get; set; } = DataTableColumnType.Text;

    /// <summary>Conditions controlling badge color. Evaluated top-to-bottom; empty Value acts as default.</summary>
    public List<BadgeCondition> BadgeConditions { get; set; } = new();

    public List<DataTableButton> Buttons { get; set; } = new();
}