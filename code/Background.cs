using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public partial class Background : Sprite
	{
		public override void Spawn()
		{
			base.Spawn();

			TexturePath = "textures/sprites/bg_square.png";
			Scale = 33f;
			Depth = -200f;
            Filter = SpriteFilter.Pixelated;
			ColorFill = new Color(0.1f, 0.3f, 0.1f);
        }

		[Event.Tick.Server]
		public void ServerTick()
		{
			//if(TempWeight > 0.01f)
			//	DebugOverlay.Text(TempWeight.ToString(), Position);

			//DebugOverlay.Text(Depth.ToString("#.##"), Position);
			//FeetOffset = 0.35f;
			//DebugOverlay.Line(Position, Position + Velocity, 0f, false);

			//Scale = 33f;

			ColorFill = new Color(0.02f, 0.02f, 0.01f);

			//Scale = 200f;
		}
	}
}
