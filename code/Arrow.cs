﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Test2D;

public partial class Arrow : Sprite
{
    public Arrow()
    {
        SpriteTexture = "textures/sprites/arrow.png";
        Scale = 0.4f;

        Filter = SpriteFilter.Pixelated;
    }
}
