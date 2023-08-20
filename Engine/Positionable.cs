﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine;

public abstract class Positionable
{
    private Vector2 _position;

    public Vector2 Position => _position;
    public float X { get => _position.X; set => _position.X = value; }
    public float Y { get => _position.Y; set => _position.Y = value; }
}