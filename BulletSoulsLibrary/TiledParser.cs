using Engine;
using Engine.Managers;
using Engine.Nodes;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

namespace BulletSoulsLibrary;

public static class TiledParser
{
    public static Node ParseMap(JuicyContentManager contentManager, JObject map)
    {
        Node mapNode = new();
        if (map["layers"] is not JArray layers)
            return mapNode;

        foreach (JObject layer in layers)
        {
            var layerType = layer.Value<string>("type");
            var layerName = layer.Value<string>("name") ?? "layer" + mapNode.CountChildren;
            switch (layerType)
            {
                case "tilelayer":
                    mapNode.AddChild(layerName, ParseTileLayer(contentManager, layer));
                    break;

                case "objectgroup":
                    mapNode.AddChild(layerName, ParseObjectLayer(contentManager, layer));
                    break;

                default:
                    throw new Exception("Unable to parse layer of type '" + layerType + "'.");
            }
        }
        return mapNode;
    }

    public static TileLayer ParseTileLayer(JuicyContentManager contentManager, JToken layer)
    {
        var name = layer.Value<string>("name") ?? "";
        var width = layer.Value<int>("width");
        var height = layer.Value<int>("height");
        var tiles = (layer["data"] as JArray)?.ToObject<int[]>();
        if (tiles is null)
        {
            tiles = new int[width * height];
        }
        var tileLayer = new TileLayer(name, width, height, tiles)
        {
            TileSet = new TileSet(32, 32, contentManager.Textures["background"])
        };
        return tileLayer;
    }

    public static Node ParseObjectLayer(JuicyContentManager contentManager, JToken layer)
    {
        Node parentNode = new();
        string nodeName = layer.Value<string>("name") ?? "Object Layer";
        int counter = 0;
        var objects = layer["objects"];
        if (objects is null)
            return parentNode;

        foreach (JObject obj in objects)
        {
            var parsedNode = ParseObject(contentManager, obj);
            var name = parsedNode.name ?? nodeName + counter++;
            parentNode.AddChild(name, parsedNode.node);
        }
        return parentNode;
    }

    public static (string name, Node node) ParseObject(JuicyContentManager contentManager, JObject obj)
    {
        string name = "";
        Node objectNode;
        var properties = ParseProperties(contentManager, obj);

        switch (obj.Value<string>("type"))
        {
            case "Player":
                name = obj.Value<string>("name") ?? "";
                properties.TryGetValue("Speed", out object? speed);
                properties.TryGetValue("DashDistance", out object? dashDistance);
                properties.TryGetValue("DashCooldown", out object? dashCooldown);
                properties.TryGetValue("SprintAcceleration", out object? sprintAcceleration);
                properties.TryGetValue("SprintBaseSpeed", out object? sprintBaseSpeed);
                properties.TryGetValue("ParryWindow", out object? parryWindow);
                objectNode = new PlayerNode()
                {
                    Position = new Vector2(obj.Value<float>("x"), obj.Value<float>("y")),
                    Speed = (float)(speed ?? 0),
                    DashDistance = (float)(dashDistance ?? 0),
                    DashCooldown = (int)(dashCooldown ?? 0),
                    SprintAcceleration = (float)(sprintAcceleration ?? 0),
                    SprintBaseSpeed = (float)(sprintBaseSpeed ?? 0),
                    ParryWindow = (int)(parryWindow ?? 0),
                };

                properties.TryGetValue("sprite", out object? spriteProperty);
                if (spriteProperty is SpriteNode spriteNode)
                    objectNode.AddChild("sprite", spriteNode);

                for (int i = 0; i < 4; i++)
                {
                    properties.TryGetValue("dashTrail" + i, out object? dashTrailProperty);
                    if (dashTrailProperty is SpriteNode dashTrail)
                        objectNode.AddChild("dashTrail" + i, dashTrail);
                }
                break;

            case "Collider":
                var position = new Vector2(obj.Value<float>("x"), obj.Value<float>("y"));
                var dimensions = new Vector2(obj.Value<float>("width"), obj.Value<float>("height"));
                objectNode = new ColliderNode(position + dimensions / 2, dimensions);
                break;

            default:
                objectNode = new();
                break;
        }
        return (name, objectNode);
    }

    public static Dictionary<string, object?> ParseProperties(JuicyContentManager contentManager, JObject obj)
    {
        var properties = new Dictionary<string, object?>();
        var propArray = obj.Value<JArray>("properties");
        if (propArray is null)
            return properties;

        foreach (JObject item in propArray)
        {
            var name = item.Value<string>("name");
            if (name == null)
                continue;
            
            var type = item.Value<string>("type");
            object? value = type switch
            {
                "bool" => item.Value<bool>("value"),
                "color" => item.Value<Color>("value"),
                "float" => item.Value<float>("value"),
                "file" => item.Value<string>("value"),
                "int" => item.Value<int>("value"),
                "object" => item.Value<int>("value"),
                "string" => item.Value<string>("value"),
                "class" => ParseClassProperty(contentManager, item),
                _ => throw new Exception("Unknown property type: " + type),
            };
            properties.Add(name, value);
        }
        return properties;
    }

    public static object ParseClassProperty(JuicyContentManager contentManager, JObject property)
    {
        var type = property.Value<string>("propertytype");
        var properties = property.Value<JObject>("value") ?? new JObject();
        switch (type)
        {
            case "Sprite":
                var x = properties.Value<float>("x");
                var y = properties.Value<float>("y");
                var spriteName = properties.Value<string>("SpriteName") ?? "";
                var animationName = properties.Value<string>("AnimationName");
                var blendColor = JuicyContentManager.FromHex(properties.Value<string>("BlendColor") ?? "#00000000");
                if (!properties.TryGetValue("Visible", out JToken? visible))
                {
                    visible = true;
                }
                contentManager.Sprites.TryGetValue(spriteName, out Sprite? sprite);
                return new SpriteNode(sprite, animationName)
                {
                    Position = new Vector2(x, y),
                    Visible = (bool)visible,
                    BlendColor = blendColor,
                };

            default:
                return new Node();
        }
    }
}
