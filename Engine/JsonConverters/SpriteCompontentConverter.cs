using Engine.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Engine.JsonConverters;

public class SpriteComponentConverter : JsonConverter
{
    public override bool CanWrite => true;
    public override bool CanRead => false;

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(SpriteComponent);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var jsonObject = new JObject
        {
            { "Type", "Sprite" },
        };
        serializer.Serialize(writer, jsonObject);
        //TODO: Make sure this works?
        throw new NotImplementedException();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new InvalidOperationException("SpriteComponent converter can only write. To read, use generic ComponentConverter.");
    }
}
