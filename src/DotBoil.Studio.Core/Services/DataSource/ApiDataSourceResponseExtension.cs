using System.Text.Json;

namespace DotBoil.Studio.Core.Services;

public static class ApiDataSourceResponseExtension
{
    public static object GetRealValue(this JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? (object)l :
                element.TryGetDouble(out var d) ? (object)d :
                element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Object => ConvertObject(element),
            JsonValueKind.Array => ConvertArray(element),
            _ => element.ToString() // JsonValueKind.Undefined vs.
        };
    }
    
    private static Dictionary<string, object?> ConvertObject(JsonElement obj)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in obj.EnumerateObject())
        {
            dict[property.Name] = property.Value.GetRealValue();
        }
        return dict;
    }

    private static List<object?> ConvertArray(JsonElement array)
    {
        var list = new List<object?>();
        foreach (var item in array.EnumerateArray())
        {
            list.Add(item.GetRealValue());
        }
        return list;
    }
}