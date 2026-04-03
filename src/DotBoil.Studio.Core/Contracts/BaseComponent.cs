using System.Text.Json.Serialization;
using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Components.Form;
using DotBoil.Studio.Core.Contracts.Rules;

namespace DotBoil.Studio.Core.Contracts;

public abstract class BaseComponent
{
    public abstract string Category { get; }
    public abstract string Title { get; }
    public abstract string ComponentIcon { get; }

    [FieldProperty(Int32.MinValue, "Id", "Unique identifier for the form field. This value is used for form processing and referencing.")]
    public string Id { get; set; }

    [FieldProperty(0, "Grid Column Span", "Defines how many grid columns this component will occupy in the layout system.", typeof(ColumnSpanSizeRenderer))]
    public ColumnSpanSize ColumnSpanSize { get; set; } = new();

    [FieldProperty(Int32.MaxValue, "Depends On", "List of field Ids that affect the visibility or behavior of this field.", typeof(DependsOnRenderer))]
    public List<string> DependsOn { get; set; } = new();

    [FieldProperty(Int32.MaxValue - 5, "Show Rules", "Rules that make this component visible when conditions are matched.", typeof(RuleSettingsRenderer))]
    public List<ComponentRule> ShowRules { get; set; } = new();

    [FieldProperty(Int32.MaxValue - 4, "Hide Rules", "Rules that hide this component when conditions are matched.", typeof(RuleSettingsRenderer))]
    public List<ComponentRule> HideRules { get; set; } = new();

    [FieldProperty(Int32.MaxValue - 3, "Enable Rules", "Rules that enable this component when conditions are matched.", typeof(RuleSettingsRenderer))]
    public List<ComponentRule> EnableRules { get; set; } = new();

    [FieldProperty(Int32.MaxValue - 2, "Disable Rules", "Rules that disable this component when conditions are matched.", typeof(RuleSettingsRenderer))]
    public List<ComponentRule> DisableRules { get; set; } = new();

    [FieldProperty(Int32.MaxValue - 1, "Validation Rules", "Rules that trigger validation messages when conditions are matched.", typeof(RuleSettingsRenderer))]
    public List<ComponentRule> ValidationRules { get; set; } = new();

    [JsonIgnore]
    public abstract Type RendererType { get; }

    [JsonIgnore]
    public bool IsVisible { get; set; } = true;

    [JsonIgnore]
    public bool IsEnabled { get; set; } = true;

    [JsonIgnore]
    public bool HasValidationError { get; set; }

    [JsonIgnore]
    public string ValidationMessage { get; set; } = string.Empty;

    [JsonIgnore]
    public Func<BaseComponent, EventArgs, Task> DependsOnHandler { get; set; }
}