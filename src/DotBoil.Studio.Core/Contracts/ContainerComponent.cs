using System.Text.Json.Serialization;

namespace DotBoil.Studio.Core.Contracts;

public abstract class ContainerComponent : BaseComponent
{
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<BaseComponent> Children { get; set; } = new();
}
