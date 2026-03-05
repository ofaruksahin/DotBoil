using System.Text.Json;

namespace DotBoil.Studio.Core.Contracts;

public class ApiDataSourceBaseResponse
{
    public List<Dictionary<string, JsonElement>> Data { get; set; }
}