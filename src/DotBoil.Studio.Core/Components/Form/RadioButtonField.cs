using System.Text.Json.Serialization;
using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;
using MudBlazor;

namespace DotBoil.Studio.Core.Components.Form;

public class RadioButtonField : DataSourceComponent
{
    public override string Category => "Form";
    public override string Title => "Radio Button";
    public override string ComponentIcon => Icons.Material.Rounded.RadioButtonChecked;
    public override Type RendererType => typeof(RadioButtonFieldRenderer);

    [FieldProperty(1, "Label", "Specifies the label text displayed above the radio button group.")]
    public string Label { get; set; }

    [FieldProperty(2, "Helper Text", "Specifies the helper or hint text displayed below the radio button group.")]
    public string HelperText { get; set; }

    [FieldProperty(3, "Text Field", "Specifies the property name of the data source item to be displayed as text in the radio button list.")]
    public string TextField { get; set; }

    [FieldProperty(4, "Value Field", "Specifies the property name of the data source item to be used as the selected value.")]
    public string ValueField { get; set; }

    [FieldOutputProperty]
    [JsonIgnore]
    public string Value { get; set; } = string.Empty;
}
