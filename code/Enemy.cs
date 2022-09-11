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
		public float FeetOffset { get; private set; }

		public (int, int) GridPos { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			//TexturePath = "textures/sprites/mummy_walk3.png";
			TexturePath = "textures/sprites/zombie.png";

			//Scale = new Vector2(1f, 35f / 16f) * 0.5f;
			//RenderColor = Color.Random;
			//Rotation = Time.Now * 0.1f;
			FeetOffset = 0.35f;

			Radius = 0.3f;
		}

		[Event.Tick.Server]
		public void ServerTick()
		{
			//DebugOverlay.Text(GridPos.ToString(), Position);
			//DebugOverlay.Text(Depth.ToString("#.#"), Position);
			//FeetOffset = 0.35f;
			//DebugOverlay.Line(Position + new Vector2(0.2f, -FeetOffset), Position + new Vector2(0.2f, 2f), 0f, false);
		}
	}
}
