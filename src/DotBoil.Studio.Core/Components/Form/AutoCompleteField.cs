using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;

namespace DotBoil.Studio.Core.Components.Form;

public class AutoCompleteField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Auto Complete";
    public override Type RendererType => typeof(AutoCompleteFieldRenderer);

    [FieldProperty(1, "Text Field", "Specifies the property name to be displayed as text in the autocomplete list.")]
    public string TextField { get; set; }
    [FieldProperty(2, "Value Field", "Specifies the property name to be used as the selected value.")]
    public string ValueField { get; set; }
    [FieldProperty(3, "Data Source", "Defines the data source that provides the items for the autocomplete component.", typeof(DataSourceSettingsRenderer))]
    public DataSource DataSource { get; set; }

    [FieldOutputProperty]
    public string Value { get; set; }
}