using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotBoil.Serialization
{
    public static class SerializationExtensions
    {
        public static async Task<string> SerializeAsync<T>(this T data)
        {
            return await Task.Run(() =>
            {
                return JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    WriteIndented = true
                });
            });
        }

        public static async Task<T> DeserializeAsync<T>(this string json)
        {
            return await Task.Run(() =>
            {
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    WriteIndented = true
                });
            });
        }
    }
}
