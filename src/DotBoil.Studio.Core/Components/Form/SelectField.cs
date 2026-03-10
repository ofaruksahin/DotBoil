using System.Text.Json.Serialization;
using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;

namespace DotBoil.Studio.Core.Components.Form;

public class SelectField : DataSourceComponent
{
    public override string Category => "Form";
    public override string Title => "Select Field";
    public override Type RendererType => typeof(SelectFieldRenderer);

    [FieldProperty(1, "Label", "Specifies the label text displayed above the select component.")]
    public string Label { get; set; }

    [FieldProperty(2, "Helper Text", "Specifies the helper or hint text displayed below the select component.")]
    public string HelperText { get; set; }
    
    [FieldProperty(3, "Placeholder", "Specifies the placeholder text displayed when no item is selected.")]
    public string PlaceHolder { get; set; }
    
    [FieldProperty(4, "Text Field", "Specifies the property name of the data source item to be displayed as text in the select list.")]
    public string TextField { get; set; }
    
    [FieldProperty(5, "Value Field", "Specifies the property name of the data source item to be used as the selected value.")]
    public string ValueField { get; set; }

    [FieldProperty(6, "Multi Selection", "Specifies the multiple selection options for the data source.")]
    public bool MultiSelection { get; set; }
    
    [FieldProperty(7, "SelectAll", "Specifies the select all options for the data source.")]
    public bool SelectAll { get; set; }

    [FieldProperty(8, "Select All Text ", "Specifies the select all text options for the data source.")]
    public string SelectAllText { get; set; }

    [FieldProperty(9, "Auto Complete Mode", "Specifies the auto complete options for the data source.")]
    public bool AutoCompleteMode { get; set; }

    [FieldOutputProperty]
    [JsonIgnore]
    public List<string> Value { get; set; }
}