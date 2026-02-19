using DotBoil.Studio.Core.Contracts;

namespace DotBoil.Studio.Core.Components.Form;

public class CheckBoxField : BaseFormField
{
    public override string Category => "Form";
    public override string Title => "Check Box";
    public override Type RendererType { get; }
}