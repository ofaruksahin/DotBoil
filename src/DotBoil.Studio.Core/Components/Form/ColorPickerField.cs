using System.Text.Json.Serialization;
using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;
using MudBlazor.Utilities;

namespace DotBoil.Studio.Core.Components.Form;

public class ColorPickerField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Color Picker";
    public override Type RendererType => typeof(ColorPickerFieldRenderer);

    [FieldProperty(1,"Label", "Label of the text field.")]
    public string Label { get; set; }

    [FieldProperty(2,"Helper Text", "Helper Text of the text field.")]
    public string HelperText { get; set; }
    
    [JsonIgnore]
    public MudColor Value { get; set; }

    [JsonIgnore]
    [FieldOutputProperty]
    public string ValueAsString => Value?.ToString();
}