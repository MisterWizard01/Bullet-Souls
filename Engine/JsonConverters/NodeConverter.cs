using Engine.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Engine.JsonConverters;

public class NodeConverter : JsonConverter
{
    public override bool CanWrite => false;
    public override bool CanRead => true;

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Nodes.Node);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new InvalidOperationException("Component converter can only read. To write use the converter specific to that component.");
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject.Value<string>("Type");
        var children = jsonObject["Children"]?.ToObject<Dictionary<string, Node>>() ?? new();
        var node = type switch
        {
            "Sprite" => new SpriteNode(children, jsonObject.Value<string>("SpriteName") ?? "", jsonObject.Value<string>("AnimationName") ?? ""),
            "Player" => new PlayerNode(children),
            _ => new Node(children),
        };
        serializer.Populate(jsonObject.CreateReader(), node);
        return node;
    }
}
