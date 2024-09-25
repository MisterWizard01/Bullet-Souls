using Engine.JsonConverters;
using Engine.Managers;
using Engine.Nodes;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

namespace BulletSoulsLibrary;

public class CustomTiledParser : TiledParser
{
    public override (string name, Node node) ParseObject(JuicyContentManager contentManager, JObject obj)
    {
        string name;
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
                properties.TryGetValue("WeaponChargeRate", out object? weaponChargeRate);
                objectNode = new PlayerNode()
                {
                    Position = new Vector2(obj.Value<float>("x"), obj.Value<float>("y")),
                    Speed = (float)(speed ?? 0),
                    DashDistance = (float)(dashDistance ?? 0),
                    DashCooldown = (int)(dashCooldown ?? 0),
                    SprintAcceleration = (float)(sprintAcceleration ?? 0),
                    SprintBaseSpeed = (float)(sprintBaseSpeed ?? 0),
                    ParryWindow = (int)(parryWindow ?? 0),
                    WeaponChargeRate = (float)(weaponChargeRate ?? 0),
                };

                properties.TryGetValue("sprite", out object? spriteProperty);
                if (spriteProperty is SpriteNode spriteNode)
                    objectNode.AddChild("sprite", spriteNode);

                properties.TryGetValue("collider", out object? colliderProperty);
                if (colliderProperty is ColliderNode colliderNode)
                    objectNode.AddChild("collider", colliderNode);

                for (int i = 0; i < 4; i++)
                {
                    properties.TryGetValue("dashTrail" + i, out object? dashTrailProperty);
                    if (dashTrailProperty is SpriteNode dashTrail)
                        objectNode.AddChild("dashTrail" + i, dashTrail);
                }
                break;

            default:
                return base.ParseObject(contentManager, obj);
        }
        return (name, objectNode);
    }
}
