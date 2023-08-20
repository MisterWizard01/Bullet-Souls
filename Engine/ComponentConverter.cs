using Engine.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
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
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            IComponent component;
            var type = jsonObject.Value<string>("Type");
            switch (type)
            {
                case "Sprite":
                    component = new SpriteComponent();
                    break;

                default:
                    Debug.WriteLine("Tried to deserialize component of type '" + type + "'.");
                    return null;
            }
            serializer.Populate(jsonObject.CreateReader(), component);
            return component;
        }
    }
}
