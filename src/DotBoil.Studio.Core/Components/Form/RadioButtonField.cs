using DotBoil.Studio.Core.Contracts;

namespace DotBoil.Studio.Core.Components.Form;

public class RadioButtonField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Radio Button";
    public override Type RendererType => typeof(RadioButtonFieldRenderer);
}