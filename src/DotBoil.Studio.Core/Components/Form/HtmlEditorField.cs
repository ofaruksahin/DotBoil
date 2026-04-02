using System.Text.Json.Serialization;
using DotBoil.Studio.Core.Contracts;
using DotBoil.Studio.Core.Attributes;
using MudBlazor;

namespace DotBoil.Studio.Core.Components.Form;

public class HtmlEditorField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Html Editor";
    public override string ComponentIcon => Icons.Material.Rounded.Code;
    public override Type RendererType => typeof(HtmlEditorFieldRenderer);

    [FieldOutputProperty]
    [JsonIgnore]
    public string Value { get; set; }
}