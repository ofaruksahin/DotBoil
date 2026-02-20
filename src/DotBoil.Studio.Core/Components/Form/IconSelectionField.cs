using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;
using MudBlazor;

namespace DotBoil.Studio.Core.Components.Form;

public class IconSelectionField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Icon Selection";
    public override Type RendererType => typeof(IconSelectionFieldRenderer);

    [FieldOutputProperty] public string Value { get; set; } = Icons.Material.Filled.Search;
}