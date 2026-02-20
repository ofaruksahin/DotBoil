using DotBoil.Studio.Core.Contracts;
using DotBoil.Studio.Core.Attributes;

namespace DotBoil.Studio.Core.Components.Form;

public class HtmlEditorField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Html Editor";
    public override Type RendererType => typeof(HtmlEditorFieldRenderer);

    [FieldOutputProperty]
    public string Value { get; set; }
}