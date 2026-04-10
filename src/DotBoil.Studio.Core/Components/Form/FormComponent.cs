using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;
using MudBlazor;

namespace DotBoil.Studio.Core.Components.Form;

public class FormComponent : ContainerComponent
{
    public override string Category => "Container";
    public override string Title => "Form";
    public override string ComponentIcon => Icons.Material.Rounded.DynamicForm;
    public override Type RendererType => typeof(FormComponentRenderer);

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [FieldProperty(1, "Action URL", "The endpoint URL where the form data will be submitted.", typeof(ApiUrlSettingsRenderer))]
    public string ActionUrl { get; set; } = string.Empty;

    [FieldProperty(2, "HTTP Method", "HTTP method used for form submission.", typeof(HttpMethodSettingsRenderer))]
    public string HttpMethod { get; set; } = "POST";

    [FieldProperty(3, "Load URL", "Endpoint URL to load existing data for update forms.", typeof(ApiUrlSettingsRenderer))]
    public string LoadUrl { get; set; } = string.Empty;

    [FieldProperty(4, "Submit Button Text", "Text displayed on the form submit button.")]
    public string SubmitButtonText { get; set; } = "Gönder";

    [FieldProperty(5, "Submit Button Icon", "Icon displayed on the submit button.", typeof(IconSelectionSettingsRenderer))]
    public Icon SubmitButtonIcon { get; set; } = new();

    [FieldProperty(6, "Success Message", "Message shown to the user after successful form submission.")]
    public string SuccessMessage { get; set; } = string.Empty;

    [FieldProperty(7, "Error Message", "Message shown to the user when form submission fails.")]
    public string ErrorMessage { get; set; } = string.Empty;

    [FieldProperty(8, "Show Cancel Button", "When enabled, a cancel button is displayed on the form.")]
    public bool CancelButtonVisible { get; set; } = false;

    [FieldProperty(9, "Cancel Button Text", "Text displayed on the cancel button.")]
    public string CancelButtonText { get; set; } = "Vazgeç";

    [FieldProperty(10, "Cancel Button Icon", "Icon displayed on the cancel button.", typeof(IconSelectionSettingsRenderer))]
    public Icon CancelButtonIcon { get; set; } = new();
}