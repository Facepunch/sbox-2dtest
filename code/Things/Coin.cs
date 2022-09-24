using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.MyGame;

namespace Sandbox
{
	public partial class Coin : Thing
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

			if (Host.IsServer)
			{
				SpriteTexture = SpriteTexture.Atlas("textures/sprites/coin.png", 3, 3);
				AnimationPath = "textures/sprites/coin_idle.frames";
				AnimationSpeed = 3f;

				Scale = new Vector2(1f, 1f) * 0.4f;
				SpawnTime = 0f;
				Damage = 10f;
				AddTempWeight = 2f;
				Force = 0.75f;
				Radius = 0.125f;
				Lifetime = 1f;

				CollideWith.Add(typeof(Enemy));
				CollideWith.Add(typeof(PlayerCitizen));
				CollideWith.Add(typeof(Coin));
			}

			Filter = SpriteFilter.Pixelated;
		}

		public override void Update(float dt)
		{
			base.Update(dt);

			Position += Velocity * dt;
			Velocity *= 0.985f;

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

		public override void Collide(Thing other, float percent, float dt)
		{
			base.Collide(other, percent, dt);

			if (other is Enemy enemy && !enemy.IsDying)
			{
				Velocity += (Position - other.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 10f) * (1f + other.TempWeight) * dt;
			} 
			else if (other is PlayerCitizen player)
			{
				Remove();
			}
			else if (other is Coin coin)
			{
				Remove();
			}
		}
	}
}
