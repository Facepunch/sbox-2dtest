using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public partial class Bullet : Sprite
	{
		public TimeSince SpawnTime { get; private set; }

		public Pawn Shooter { get; set; }

		public float Damage { get; private set; }

		public override void Spawn()
		{
			base.Spawn();

			TexturePath = "textures/sprites/bullet.png";

			//RenderColor = Color.Random;

			Scale = new Vector2(0.2f, 0.2f);

			SpawnTime = 0f;

			Damage = 15f;
		}

		[Event.Tick.Server]
		public void ServerTick()
        {
			Position += (Vector2)Velocity * Time.Delta;

			if (SpawnTime > 1f)
				Delete();

			var players = Entity.All.OfType<Pawn>().ToList();

			foreach(var p in players)
            {
				if (p == Shooter)
					continue;

				if ((p.Position - Position).LengthSquared < 0.5f * 0.5f)
                {
					p.Damage(Damage);
					Delete();
				}
            }
        }
	}
}
