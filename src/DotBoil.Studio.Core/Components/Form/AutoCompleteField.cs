using DotBoil.Studio.Core.Contracts;

namespace DotBoil.Studio.Core.Components.Form;

public class AutoCompleteField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Auto Complete";
    public override Type RendererType => typeof(AutoCompleteFieldRenderer);
}