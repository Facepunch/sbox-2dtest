using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public partial class Enemy : Sprite
	{
		public float Radius { get; private set; }

		public override void Spawn()
		{
			base.Spawn();

            TexturePath = "textures/sprites/mummy_walk3.png";

			//Scale = new Vector2(1f, 35f / 16f) * 0.5f;
			//RenderColor = Color.Random;
			//Rotation = Time.Now * 0.1f;

			Radius = 0.3f;
		}
	}
}
