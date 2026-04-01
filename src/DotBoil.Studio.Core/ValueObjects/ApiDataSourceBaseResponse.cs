using System.Text.Json;

namespace DotBoil.Studio.Core.ValueObjects;

public class ApiDataSourceBaseResponse
{
    public List<Dictionary<string, JsonElement>> Data { get; set; }
}