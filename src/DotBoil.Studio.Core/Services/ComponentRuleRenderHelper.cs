using DotBoil.Studio.Core.Contracts;
using DotBoil.Studio.Core.ValueObjects;

namespace DotBoil.Studio.Core.Services;

public static class ComponentRuleRenderHelper
{
    public static void Initialize(BaseComponent current, ComponentContext context, Func<BaseComponent, EventArgs, Task> onDepends)
    {
        if (current.DependsOnHandler == null)
            current.DependsOnHandler += onDepends;

        if (!context.DesignerMode)
            ComponentRuleEvaluator.Apply(current, context);
    }

    public static void Dispose(BaseComponent current, ComponentContext context,
        Func<BaseComponent, EventArgs, Task> onDepends)
    {
        if (current.DependsOnHandler != null)
            current.DependsOnHandler -= onDepends;
    }

    public static void ApplyOnDepends(BaseComponent current, ComponentContext context)
    {
        if (context.DesignerMode)
            return;

        ComponentRuleEvaluator.Apply(current, context);
    }
}
