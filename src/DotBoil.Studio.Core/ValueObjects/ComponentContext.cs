using DotBoil.Studio.Core.Contracts;
using Microsoft.AspNetCore.Components;

namespace DotBoil.Studio.Core.ValueObjects;

public class ComponentContext
{
    public bool DesignerMode { get; set; }
    public List<BaseComponent> Components { get; set; }
    public List<string> UserRoles { get; set; } = new();
    public EventCallback<BaseComponent> OnFieldSelected { get; set; }

    public async Task OnDepends(BaseComponent component)
    {
        var refs = Components.Where(c =>
            c.DependsOn != null &&
            c.DependsOn.Contains(component.Id));

        foreach (var item in refs)
        {
            if (item.DependsOnHandler != null)
                item.DependsOnHandler.Invoke(component, EventArgs.Empty);
        }
    }
}