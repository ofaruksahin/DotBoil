using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;
using MudBlazor;

namespace DotBoil.Studio.Core.Components.Form;

public class EmailField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Email Field";
    public override Type RendererType => typeof(EmailFieldRenderer);

    [FieldProperty(1,"Label", "Label of the text field.")]
    public string Label { get; set; }
    
    [FieldProperty(2,"Helper Text", "Helper Text of the text field.")]
    public string HelperText { get; set; }

    [FieldProperty(3,"Icon", "Icon of the text field.", typeof(IconSelectionSettingsRenderer))]
    public Icon Icon { get; set; } = new Icon()
    {
        Value = Icons.Material.Filled.Email
    };

    [FieldOutputProperty]
    public string Value { get; set; }
}