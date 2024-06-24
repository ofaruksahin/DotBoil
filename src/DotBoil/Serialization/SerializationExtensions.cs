using System.Text;
using System.Text.Json;

namespace DotBoil.Serialization
{
    public static class SerializationExtensions
    {
        public static async Task<string> SerializeAsync<T>(this T data)
        {
            await using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, data);
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        public static async Task<T> DeserializeAsync<T>(this string json)
        {
            var byteArray = Encoding.UTF8.GetBytes(json);
            await using var stream = new MemoryStream(byteArray);
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }
    }
}
