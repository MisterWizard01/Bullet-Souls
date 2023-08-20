using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Components;

public struct SpriteComponent : IComponent
{
    public Vector2 Offset { get; set; }
    public string? SpriteName { get; set; }
    public string? AnimationName { get; set; }
    public float FrameRatio { get; set; }
    public float FrameIndex { get; set; }
    public Color BlendColor { get; set; }

    public SpriteComponent()
    {
        Offset = Vector2.Zero;
        SpriteName = null;
        AnimationName = null;
        FrameRatio = 1;
        FrameIndex = 0;
        BlendColor = Color.White;
    }
}
