using Engine.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Engine.JsonConverters;

public class ComponentConverter : JsonConverter
{
    public override bool CanWrite => false;
    public override bool CanRead => true;

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(IComponent);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new InvalidOperationException("Component converter can only read. To write use the converter specific to that component.");
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        IComponent component;
        var type = jsonObject.Value<string>("Type");
        switch (type)
        {
            case "Sprite":
                component = new SpriteComponent(jsonObject.Value<string>("SpriteName") ?? "", jsonObject.Value<string>("AnimationName") ?? "");
                break;

            case "PlayerController":
                component = new PlayerComponent(jsonObject.Value<float>("Speed"));
                break;

            default:
                Debug.WriteLine("Tried to deserialize component of type '" + type + "'.");
                return null;
        }
        serializer.Populate(jsonObject.CreateReader(), component);
        return component;
    }
}
