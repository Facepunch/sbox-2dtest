using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public partial class Arrow : Sprite
	{
		public override void Spawn()
		{
			base.Spawn();

			TexturePath = "textures/sprites/arrow.png";
			Scale = 0.4f;

            Filter = SpriteFilter.Pixelated;
        }
	}
}
