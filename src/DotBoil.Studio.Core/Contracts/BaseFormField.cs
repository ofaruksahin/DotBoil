using System.Text.Json.Serialization;
using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Components.Form;

namespace DotBoil.Studio.Core.Contracts;

public abstract class BaseFormField
{
    public abstract string Category { get; }
    public abstract string Title { get; }
    
    public abstract Type RendererType { get; }

    [FieldProperty(Int32.MinValue, "Id", "Unique identifier for the form field. This value is used for form processing and referencing.")]
    public string Id { get; set; }

    // [FieldProperty( "", "", typeof(ColumnSpanSizeRenderer))]
    public ColumnSpanSize ColumnSpanSize { get; set; } = new();

    [FieldProperty(Int32.MaxValue, "Depends On", "List of field Ids that affect the visibility or behavior of this field.")]
    public List<string> DependsOn { get; set; } = new();
}