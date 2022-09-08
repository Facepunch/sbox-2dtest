using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public partial class Mummy : Sprite
	{
		public override void Spawn()
		{
			base.Spawn();

			TexturePath = "textures/sprites/mummy_walk3.png";

			RenderColor = Color.Random;
		}

		[Event.Tick.Server]
		public void ServerTick()
		{
			Rotation = Time.Now * 8f;
		}
	}
}
