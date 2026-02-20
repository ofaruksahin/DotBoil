using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;
using MudBlazor;
using MudBlazor.Utilities;

namespace DotBoil.Studio.Core.Components.Form;

public class CheckBoxField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Check Box";
    public override Type RendererType => typeof(CheckBoxFieldRenderer);

    [FieldProperty(1, "Label", "The text displayed next to the checkbox.")]
    public string Label { get; set; }

    [FieldProperty(2, "Checked Icon", "The icon displayed when the checkbox is checked.",
        typeof(IconSelectionSettingsRenderer))]
    public Icon CheckedIcon { get; set; } = new Icon()
    {
        Value = Icons.Material.Filled.CheckBox
    };

    [FieldProperty(3, "Unchecked Icon", "The icon displayed when the checkbox is unchecked.",
        typeof(IconSelectionSettingsRenderer))]
    public Icon UncheckedIcon { get; set; } = new Icon()
    {
        Value = Icons.Material.Filled.CheckBoxOutlineBlank
    };

    [FieldOutputProperty] public bool Value { get; set; }
}