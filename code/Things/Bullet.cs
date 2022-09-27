using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.MyGame;

namespace Sandbox
{
	public partial class Bullet : Thing
	{
		public TimeSince SpawnTime { get; private set; }

		public PlayerCitizen Shooter { get; set; }

		public float Damage { get; set; }
		public float Force { get; set; }
		public float AddTempWeight { get; set; }
		public float Lifetime { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			if(Host.IsServer)
            {
				SpriteTexture = "textures/sprites/bullet.png";

				//RenderColor = Color.Random;

				Scale = new Vector2(0.2f, 0.2f);
				SpawnTime = 0f;
				Damage = 10f;
				AddTempWeight = 2f;
				Force = 0.75f;
				Radius = 0.1f;
				Pivot = new Vector2(0.5f, -1.2f);
				Lifetime = 1f;

				CollideWith.Add(typeof(Enemy));
			}

			Filter = SpriteFilter.Pixelated;
		}

		public override void Update(float dt)
		{
			base.Update(dt);

			Position += Velocity * dt;
			Depth = -Position.y * 10f;

			if (SpawnTime > Lifetime)
            {
				Remove();
				return;
			}

			var gridPos = Game.GetGridSquareForPos(Position);
			if (gridPos != GridPos)
			{
				Game.DeregisterThingGridSquare(this, GridPos);
				Game.RegisterThingGridSquare(this, gridPos);
				GridPos = gridPos;
			}

			for (int dx = -1; dx <= 1; dx++)
			{
				for (int dy = -1; dy <= 1; dy++)
				{
					Game.HandleThingCollisionForGridSquare(this, new GridSquare(GridPos.x + dx, GridPos.y + dy), dt);

					if (IsRemoved)
						return;
				}
			}
        }

		public override void Colliding(Thing other, float percent, float dt)
		{
			base.Colliding(other, percent, dt);

			if (other is Enemy enemy && !enemy.IsDying && (!enemy.IsSpawning || enemy.ElapsedTime > 1.5f))
			{
				enemy.Damage(Damage, Shooter);

				enemy.Velocity += Velocity.Normal * Force;
				enemy.TempWeight += AddTempWeight;

				Remove();
                return;
			}
		}
	}
}
