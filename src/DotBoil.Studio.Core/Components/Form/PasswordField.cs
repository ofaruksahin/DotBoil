using System.Text.Json.Serialization;
using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;
using MudBlazor;

namespace DotBoil.Studio.Core.Components.Form;

public class PasswordField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Password Field";
    public override Type RendererType => typeof(PasswordFieldRenderer);
    
    [FieldProperty(1,"Label", "Label of the text field.")]
    public string Label { get; set; }
    
    [FieldProperty(2,"Helper Text", "Helper Text of the text field.")]
    public string HelperText { get; set; }

    [FieldProperty(3, "Show Password Icon", "Show Password Icon of the text field.",
        typeof(IconSelectionSettingsRenderer))]
    public Icon ShowPasswordIcon { get; set; } = new Icon()
    {
        Value = Icons.Material.Filled.Visibility
    };

    [FieldProperty(4, "Hide Password Icon", "Hide Password Icon of the text field.",
        typeof(IconSelectionSettingsRenderer))]
    public Icon HidePasswordIcon { get; set; } = new Icon()
    {
        Value = Icons.Material.Filled.VisibilityOff
    };

    [JsonIgnore]
    [FieldOutputProperty]
    public string Value { get; set; }
}