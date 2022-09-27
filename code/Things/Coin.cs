﻿using System;
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

		public Coin()
        {

        }

		public override void Spawn()
		{
			base.Spawn();

			if (Host.IsServer)
			{
				SpriteTexture = SpriteTexture.Atlas("textures/sprites/xp.png", 5, 4);
				AnimationPath = "textures/sprites/xp_1.frames";
				AnimationSpeed = 3f;

				Scale = new Vector2(1f, 1f) * 0.4f;
				SpawnTime = 0f;
				Radius = 0.125f;

				CollideWith.Add(typeof(Enemy));
				CollideWith.Add(typeof(PlayerCitizen));
				CollideWith.Add(typeof(Coin));

                SetValue(1);
				//SetValue(Rand.Int(1, 7));
			}

			Filter = SpriteFilter.Pixelated;
		}

		[Event.Tick.Client]
		public void ClientTick()
		{
			//DebugText("\nclient: " + AnimationSpeed.ToString());
		}

		[Event.Tick.Server]
		public void ServerTick()
		{
			//DebugText("server: " + AnimationSpeed.ToString());
			//DebugText(IsAttacking.ToString());
			//DebugText(SinceSpawning.Absolute.ToString("#.##"));
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

			//DebugText(Value.ToString());
		}

		public override void Colliding(Thing other, float percent, float dt)
		{
			base.Colliding(other, percent, dt);

			if (other is Enemy enemy && !enemy.IsDying)
			{
				Velocity += (HitboxPos - other.HitboxPos).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * 20f * (1f + other.TempWeight) * dt;
			} 
			else if (other is PlayerCitizen player)
			{
				player.AddExperience(Value);
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

			int tier_shape = 1 + MathX.FloorToInt((value - 1)/ 5f);
			int tier_color = value % 5;

			switch (tier_shape)
            {
				case 1:
					AnimationPath = "textures/sprites/xp_1.frames";
					break;
				case 2:
					AnimationPath = "textures/sprites/xp_2.frames";
					break;
				case 3:
					AnimationPath = "textures/sprites/xp_3.frames";	
					break;
				case 4:
					AnimationPath = "textures/sprites/xp_4.frames";
					break;
				case 5:
				default:
					AnimationPath = "textures/sprites/xp_5.frames";
					break;
			}

			switch (tier_color)
			{
				case 1:
                    ColorFill = new Color(0.2f, 0.2f, 1f) * 1.3f;
					break;
				case 2:
					ColorFill = new Color(1f, 0.2f, 0.2f) * 2f;
					break;
				case 3:
					ColorFill = new Color(1f, 1f, 0.2f) * 2.3f;
					break;
				case 4:
					ColorFill = new Color(0.2f, 1f, 0.3f) * 1.7f;
					break;
				case 5:
				default:
					ColorFill = new Color(1f, 1f, 1f) * 1.8f;
					break;
			}
		}
	}
}