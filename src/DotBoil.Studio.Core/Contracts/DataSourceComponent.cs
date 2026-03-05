using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Components.Form;

namespace DotBoil.Studio.Core.Contracts;

public abstract class DataSourceComponent : BaseComponent
{
    [FieldProperty(Int32.MaxValue - 1, "Data Source",
        "Defines the data source configuration that provides the items for the select component.",
        typeof(DataSourceSettingsRenderer))]
    public DataSource DataSource { get; set; }
    
    public Func<object?, EventArgs, Task> DataSourceChangeHandler { get; set; }

    public async Task DataSourceChanged()
    {
        if (DataSourceChangeHandler != null)
            await DataSourceChangeHandler(new {}, EventArgs.Empty);
    }
}