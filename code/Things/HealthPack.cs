using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;
public partial class HealthPack : Thing
{
	public TimeSince SpawnTime { get; private set; }

	public PlayerCitizen Shooter { get; set; }

	public float Lifetime { get; set; }

	public float HpAmount { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		if(Sandbox.Game.IsServer)
        {
			SpriteTexture = "textures/sprites/health_pack.png";
			ColorTint = new Color(1f, 1f, 1f, 1f);

			SpawnTime = 0f;
			Radius = 0.175f;
			BasePivotY = 0.275f;
			HeightZ = 0f;
			Lifetime = 60f;
			ShadowOpacity = 0.8f;
			ShadowScale = 0.8f;
			Scale = new Vector2(0.6f, 0.6f);

			HpAmount = 20f;

			CollideWith.Add(typeof(Enemy));
			CollideWith.Add(typeof(PlayerCitizen));
		}

		Filter = SpriteFilter.Pixelated;
	}

    public override void ClientSpawn()
    {
        base.ClientSpawn();

		SpawnShadow(1f);
	}

	public override void Update(float dt)
	{
		if (Game.IsGameOver)
			return;

		base.Update(dt);

		Position += Velocity * dt;
		Position = new Vector2(MathX.Clamp(Position.x, Game.BOUNDS_MIN.x + Radius, Game.BOUNDS_MAX.x - Radius), MathX.Clamp(Position.y, Game.BOUNDS_MIN.y + Radius, Game.BOUNDS_MAX.y - Radius));
		Velocity *= (1f - dt * 3.5f);

		Scale = new Vector2(0.6f + Utils.FastSin(SpawnTime * 8f) * 0.025f, 0.6f + MathF.Cos(SpawnTime * 8f) * 0.025f);
		ShadowScale = 0.8f + Utils.FastSin(SpawnTime * 8f) * 0.025f;

		Depth = -Position.y * 10f;

		// todo: blink near lifetime end
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

		if(SpawnTime > 0.1f)
        {
			foreach (PlayerCitizen player in Game.AlivePlayers)
			{
				var dist_sqr = (Position - player.Position).LengthSquared;
				var req_dist_sqr = MathF.Pow(player.Stats[StatType.CoinAttractRange], 2f);
				if (dist_sqr < req_dist_sqr)
				{
					Velocity += (player.Position - Position).Normal * Utils.Map(dist_sqr, req_dist_sqr, 0f, 0f, 1f, EasingType.Linear) * player.Stats[StatType.CoinAttractStrength] * dt;
				}
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
    }

	public override void Colliding(Thing other, float percent, float dt)
	{
		base.Colliding(other, percent, dt);

		if (other is Enemy enemy && !enemy.IsDying)
		{
			Velocity += (Position - other.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * 20f * (1f + other.TempWeight) * dt;
		}
		else if (other is PlayerCitizen player)
		{
			if (!player.IsDead && SpawnTime > 0.1f)
			{
				player.Heal(HpAmount);
				Game.PlaySfxNearby("heal", Position, pitch: Utils.Map(player.Health / player.Stats[StatType.MaxHp], 0f, 1f, 1.5f, 1f), volume: 1.5f, maxDist: 5f);
				Remove();
			}
		}
	}
}
