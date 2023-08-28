using Engine.Nodes;
using Engine.CustomEventArgs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Engine.Managers;

public class SpriteManager
{
    public Dictionary<string, Sprite> Sprites { get; set; }

    public SpriteManager()
    {
        Sprites = new();
        SpriteNode.SpriteUpdateEvent += OnSpriteUpdate;
    }

    public void LoadSpriteTextures(ContentManager content)
    {
        foreach (var sprite in Sprites.Values)
        {
            sprite.Texture = content.Load<Texture2D>(sprite.TextureName);
        }
    }

    public void OnSpriteUpdate(object? sender, SpriteUpdateEventArgs e)
    {
        if (sender is not SpriteNode sc)
            return;
        if (!Sprites.TryGetValue(e.SpriteName, out var sprite))
        {
            Debug.WriteLine("Couldn't find a sprite called '" + e.SpriteName + "'.");
        }
        sc.Sprite = sprite;
    }

    public static string DirectionString8(Vector2 vector)
    {
        var direction = Math.Atan2(vector.Y, vector.X);
        var piOver8 = Math.PI / 8;
        if (direction > 7 * piOver8)
            return "left";
        if (direction > 5 * piOver8)
            return "downleft";
        if (direction > 3 * piOver8)
            return "down";
        if (direction > piOver8)
            return "downright";
        if (direction > -piOver8)
            return "right";
        if (direction > -3 * piOver8)
            return "upright";
        if (direction > -5 * piOver8)
            return "up";
        if (direction > -7 * piOver8)
            return "upleft";
        return "left";
    }

    public static string DirectionString4(Vector2 vector)
    {
        var direction = Math.Atan2(vector.Y, vector.X);
        var piOver4 = Math.PI / 4;
        if (direction > 3 * piOver4)
            return "left";
        if (direction > piOver4)
            return "down";
        if (direction > -piOver4)
            return "right";
        if (direction > -3 * piOver4)
            return "up";
        return "left";
    }

    public static void YSortChildren(Node node)
    {
        var nodeList = node.GetChildren().ToList();
        for (int i = 0; i < nodeList.Count - 1; i++)
        {
            if (nodeList[i].Value is not PositionableNode first)
                continue;
            var firstY = first.WorldPosition(node).Y;

            for (int j = i + 1; j < nodeList.Count; j++)
            {
                if (nodeList[j].Value is not PositionableNode second)
                    continue;
                var secondY = second.WorldPosition(node).Y;

                if (firstY > secondY)
                {
                    (nodeList[j], nodeList[i]) = (nodeList[i], nodeList[j]);
                    firstY = secondY;
                }
            }
        }
        node.SetChildren(new Dictionary<string, Node>(nodeList));
    }
}
