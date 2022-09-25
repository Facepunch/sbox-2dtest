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

		//public float Lifetime { get; set; }

		public int Value { get; set; }

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
				Radius = 0.125f;

				CollideWith.Add(typeof(Enemy));
				CollideWith.Add(typeof(PlayerCitizen));
				CollideWith.Add(typeof(Coin));

				SetValue(1);
			}

			Filter = SpriteFilter.Pixelated;
		}

		public override void Update(float dt)
		{
			base.Update(dt);

			Position += Velocity * dt;
			HitboxPos = new Vector2(MathX.Clamp(HitboxPos.x, Game.BOUNDS_MIN.x + Radius, Game.BOUNDS_MAX.x - Radius), MathX.Clamp(HitboxPos.y, Game.BOUNDS_MIN.y + Radius, Game.BOUNDS_MAX.y - Radius));
			Velocity *= (1f - dt * 0.92f);

			Depth = -HitboxPos.y * 10f;

			var gridPos = Game.GetGridSquareForPos(HitboxPos);
			if (gridPos != GridPos)
			{
				Game.DeregisterThingGridSquare(this, GridPos);
				Game.RegisterThingGridSquare(this, gridPos);
				GridPos = gridPos;
			}

			foreach (PlayerCitizen player in Game.AlivePlayers)
            {
				var dist_sqr = (HitboxPos - player.HitboxPos).LengthSquared;
				var req_dist_sqr = MathF.Pow(player.CoinAttractRange, 2f);
				if (dist_sqr < req_dist_sqr)
				{
					Velocity += (player.HitboxPos - HitboxPos).Normal * Utils.Map(dist_sqr, req_dist_sqr, 0f, 0f, 1f, EasingType.Linear) * player.CoinAttractStrength * dt;
				}
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

			DebugText(Value.ToString());
		}

		public override void Collide(Thing other, float percent, float dt)
		{
			base.Collide(other, percent, dt);

			if (other is Enemy enemy && !enemy.IsDying)
			{
				Velocity += (HitboxPos - other.HitboxPos).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * 20f * (1f + other.TempWeight) * dt;
			} 
			else if (other is PlayerCitizen player)
			{
				Remove();
			}
			else if (other is Coin coin)
			{
				SetValue(Value + coin.Value);
				coin.Remove();
			}
		}

		public void SetValue(int value)
        {
			Value = value;
        }
	}
}
