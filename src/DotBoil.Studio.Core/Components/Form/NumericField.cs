using System.Text.Json.Serialization;
using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;
using MudBlazor;

namespace DotBoil.Studio.Core.Components.Form;

public class NumericField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Numeric Field";
    public override string ComponentIcon => Icons.Material.Rounded.Numbers;
    public override Type RendererType => typeof(NumericFieldRenderer);

    [FieldProperty(1,"Label", "Label of the text field.")]
    public string Label { get; set; }

    [FieldProperty(2,"Helper Text", "Helper Text of the text field.")]
    public string HelperText { get; set; }

    [FieldProperty(3,"Minimum", "Minimum allowed value.")]
    public decimal? Min { get; set; }

    [FieldProperty(4,"Maximum", "Maximum allowed value.")]
    public decimal? Max { get; set; }

    [FieldProperty(5,"Step", "Increment step value.")]
    public decimal? Step { get; set; }
    
    [JsonIgnore]
    [FieldOutputProperty]
    public decimal? Value { get; set; }
}