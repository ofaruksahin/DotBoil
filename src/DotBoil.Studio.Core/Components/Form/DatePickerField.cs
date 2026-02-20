using DotBoil.Studio.Core.Contracts;

namespace DotBoil.Studio.Core.Components.Form;

public class DatePickerField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Date Picker";
    public override Type RendererType => typeof(DatePickerFieldRenderer);
}