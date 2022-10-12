using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Test2D;

public partial class BackgroundTile : Sprite
{
	public override void Spawn()
	{
		base.Spawn();

		SpriteTexture = "textures/sprites/bg_square_2.png";
        Filter = SpriteFilter.Pixelated;
		//ColorTint = new Color(0.04f, 0.11f, 0.03f, 0.2f);
		ColorTint = new Color(0.07f, 0.08f, 0.03f, 0.2f);
	}
}
