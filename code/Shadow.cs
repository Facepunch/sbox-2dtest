using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public partial class Shadow : Sprite
	{
		public override void Spawn()
		{
			base.Spawn();

			TexturePath = "textures/sprites/shadow3.png";
			Depth = -50f;
			Scale = 1f;
			ColorFill = new Color(0f, 0f, 0f);
			Filter = SpriteFilter.Pixelated;
        }
	}
}
