namespace DotBoil.Studio.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class FieldPropertyAttribute : Attribute
{
    public string Name { get; set; }
    
    public string Description { get; set; }

    public int Order { get; set; }

    public Type? RendererType { get; set; }

    public FieldPropertyAttribute(int order, string name, string description)
    {
        Order = order;
        Name = name;
        Description = description;
    }
    
    public FieldPropertyAttribute(int order, string name, string description, Type rendererType)
    {
        Order = order;
        Name = name;
        Description = description;
        RendererType = rendererType;
    }
}