using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;
public partial class Fire : Thing
{
	public TimeSince SpawnTime { get; private set; }

	public PlayerCitizen Shooter { get; set; }

	public float Lifetime { get; set; }

	private TimeSince _sinceDamageTime;
	private const float DAMAGE_INTERVAL = 0.4f;


	public override void Spawn()
	{
		base.Spawn();

		if(Sandbox.Game.IsServer)
        {
			SpriteTexture = SpriteTexture.Atlas("textures/sprites/fire_spritesheet.png", 1, 4);
			AnimationPath = "textures/sprites/fire.frames";
			AnimationSpeed = Sandbox.Game.Random.Float(3f, 6f);
			ColorTint = new Color(1f, 1f, 1f, 1f);

			SpawnTime = 0f;
			Radius = 0.1f;
			BasePivotY = 0.35f;
			HeightZ = 0f;
			Lifetime = 2f;
			ShadowOpacity = 0.8f;
			ShadowScale = 0.3f;

			CollideWith.Add(typeof(Enemy));
			CollideWith.Add(typeof(PlayerCitizen));
		}

		Filter = SpriteFilter.Pixelated;
	}

    public override void ClientSpawn()
    {
        base.ClientSpawn();

	}

	public override void Update(float dt)
	{
		if (Game.IsGameOver)
			return;

		base.Update(dt);

		Depth = -Position.y * 10f;
		bool flip = Utils.FastSin(Time.Now * 4f) < 0f;
		Scale = new Vector2((1f + Utils.FastSin(Time.Now * 24f) * 0.1f) * (flip ? -1f : 1f), 1f + Utils.FastSin(Time.Now * 14f) * 0.075f);
		Opacity = (0.6f + Utils.FastSin(Time.Now * 20f) * 0.4f) * Utils.Map(SpawnTime, Lifetime - 0.25f, Lifetime, 1f, 0f);

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

		if (Shooter == null)
			return;

		if (typeof(Enemy).IsAssignableFrom(other.GetType()))
		{
			var enemy = (Enemy)other;

			if(!enemy.IsDying && (!enemy.IsSpawning || enemy.ElapsedTime > 1.0f))
            {
				if (!enemy.HasEnemyStatus<BurningEnemyStatus>())
                {
					enemy.Burn(Shooter, Shooter.FireDamage * Shooter.GetDamageMultiplier(), Shooter.FireLifetime, Shooter.FireSpreadChance);
					Game.PlaySfxNearby("burn", Position, pitch: Sandbox.Game.Random.Float(0.95f, 1.15f), volume: 1f, maxDist: 5f);
				}
			}
		}
		else if (other is PlayerCitizen player && !player.IsDead)
		{
			if (_sinceDamageTime > DAMAGE_INTERVAL)
			{
				player.Damage(Shooter.FireDamage * Shooter.GetDamageMultiplier());
				_sinceDamageTime = 0f;
			}
		}
	}
}
