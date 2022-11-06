using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class Coin : Thing
{
	public TimeSince SpawnTime { get; private set; }

	//public float Lifetime { get; set; }

	public bool IsMagnetized { get; private set; }
	public PlayerCitizen PlayerMagnetized { get; private set; }
	public TimeSince MagnetizeTime { get; private set; }
	private const float MAGNETIZE_DURATION = 2.4f;

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
			BasePivotY = 0.225f;
			HeightZ = 0f;
			//Pivot = new Vector2(0.5f, 0.225f);

			Scale = new Vector2(1f, 1f) * 0.4f;
			SpawnTime = 0f;
			Radius = 0.125f;

			CollideWith.Add(typeof(Enemy));
			CollideWith.Add(typeof(PlayerCitizen));
			CollideWith.Add(typeof(Coin));

			ShadowOpacity = 0.8f;
			ShadowScale = 0.4f;
			//SetValue(Rand.Int(1, 7));
		}

		Filter = SpriteFilter.Pixelated;
	}

    public override void ClientSpawn()
    {
        base.ClientSpawn();

		SpawnShadow(0.4f);
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
		if (Game.IsGameOver)
			return;

		base.Update(dt);

		if(IsMagnetized)
        {
			if (MagnetizeTime > MAGNETIZE_DURATION || PlayerMagnetized == null || !PlayerMagnetized.IsValid || PlayerMagnetized.IsDead)
			{
				IsMagnetized = false;
				PlayerMagnetized = null;
			}
			else
            {
				Velocity += (PlayerMagnetized.Position - Position).Normal * 0.125f * Utils.Map(MagnetizeTime, 0f, MAGNETIZE_DURATION, 1f, 0f, EasingType.QuadIn);
            }
        }

		Position += Velocity * dt;
		Position = new Vector2(MathX.Clamp(Position.x, Game.BOUNDS_MIN.x + Radius, Game.BOUNDS_MAX.x - Radius), MathX.Clamp(Position.y, Game.BOUNDS_MIN.y + Radius, Game.BOUNDS_MAX.y - Radius));
		Velocity *= (1f - dt * 0.92f);

		Depth = -Position.y * 10f;

		var gridPos = Game.GetGridSquareForPos(Position);
		if (gridPos != GridPos)
		{
			Game.DeregisterThingGridSquare(this, GridPos);
			Game.RegisterThingGridSquare(this, gridPos);
			GridPos = gridPos;
		}

		foreach (PlayerCitizen player in Game.AlivePlayers)
        {
			var dist_sqr = (Position - player.Position).LengthSquared;
			var req_dist_sqr = MathF.Pow(player.CoinAttractRange, 2f);
			if (dist_sqr < req_dist_sqr)
			{
				Velocity += (player.Position - Position).Normal * Utils.Map(dist_sqr, req_dist_sqr, 0f, 0f, 1f, EasingType.Linear) * player.CoinAttractStrength * dt;
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
			Velocity += (Position - other.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * 20f * (1f + other.TempWeight) * dt;
		} 
		else if (other is PlayerCitizen player)
		{
			if(!player.IsDead)
            {
				player.AddExperience(Value);
				Game.PlaySfxNearby("xp", Position, pitch: Utils.Map(Value, 1, 10, 0.7f, 1.5f, EasingType.QuadIn), volume: 1f, maxDist: 6f);
				Remove ();
			}
		}
		else if (other is Coin coin)
		{
			SetValue(Value + coin.Value);
			SpawnCloudClient(coin.Position, Vector2.Zero);

			if(!IsMagnetized && coin.IsMagnetized)
            {
				PlayerMagnetized = coin.PlayerMagnetized;
				IsMagnetized = true;
				MagnetizeTime = coin.MagnetizeTime;
            }

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
				BasePivotY = 0.225f;
				//Pivot = new Vector2(0.5f, 0.225f);
				break;
			case 2:
				AnimationPath = "textures/sprites/xp_2.frames";
				BasePivotY = 0.225f;
				//Pivot = new Vector2(0.5f, 0.2f);
				break;
			case 3:
				AnimationPath = "textures/sprites/xp_3.frames";
				BasePivotY = 0.15f;
				//Pivot = new Vector2(0.5f, 0.15f);
				break;
			case 4:
				AnimationPath = "textures/sprites/xp_4.frames";
				BasePivotY = 0.1f;
				//Pivot = new Vector2(0.5f, 0.1f);
				break;
			case 5:
			default:
				AnimationPath = "textures/sprites/xp_5.frames";
				BasePivotY = 0.05f;
				//Pivot = new Vector2(0.5f, 0.05f);
				break;
		}

		HeightZ = 0f;

		switch (tier_color)
		{
			case 1:
                ColorTint = new Color(0.2f, 0.2f, 1f);
				break;
			case 2:
				ColorTint = new Color(1f, 0.2f, 0.2f);
				break;
			case 3:
				ColorTint = new Color(1f, 1f, 0.2f);
				break;
			case 4:
				ColorTint = new Color(0.2f, 1f, 0.3f);
				break;
			case 5:
			default:
				ColorTint = new Color(1f, 1f, 1f) * 2f;
				break;
		}
	}

	public void Magnetize(PlayerCitizen player)
    {
		PlayerMagnetized = player;
		IsMagnetized = true;
		MagnetizeTime = 0f;
    }
}
