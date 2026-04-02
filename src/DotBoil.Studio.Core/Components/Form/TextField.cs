using System.Text.Json.Serialization;
using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;
using MudBlazor;

namespace DotBoil.Studio.Core.Components.Form;

public class TextField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Text Field";
    public override string ComponentIcon => Icons.Material.Rounded.TextFields;
    public override Type RendererType => typeof(TextFieldRenderer);

    [FieldProperty(1,"Label", "Label of the text field.")]
    public string Label { get; set; }
    
    [FieldProperty(2,"Helper Text", "Helper Text of the text field.")]
    public string HelperText { get; set; }

    [JsonIgnore]
    [FieldOutputProperty]
    public string Value { get; set; }
}