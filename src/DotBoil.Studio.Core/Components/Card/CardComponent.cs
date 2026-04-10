using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;
using MudBlazor;

namespace DotBoil.Studio.Core.Components.Card;

public class CardComponent : ContainerComponent
{
    public override string Category => "Container";
    public override string Title => "Card";
    public override string ComponentIcon => Icons.Material.Rounded.CreditCard;
    public override Type RendererType => typeof(CardComponentRenderer);

    [FieldProperty(1, "Header", "Title text displayed at the top of the card.")]
    public string Header { get; set; } = string.Empty;

    [FieldProperty(2, "Sub Title", "Subtitle text displayed below the header.")]
    public string SubTitle { get; set; } = string.Empty;
}