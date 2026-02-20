using Microsoft.AspNetCore.Components;

namespace DotBoil.Studio.Core.Contracts;

public class ComponentContext
{
    public bool DesignerMode { get; set; }
    public List<BaseComponent> Components { get; set; }
    public EventCallback<BaseComponent> OnFieldSelected { get; set; }
}