using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;
public partial class Bullet : Thing
{
	public TimeSince SpawnTime { get; private set; }

	public PlayerCitizen Shooter { get; set; }

	public float Damage { get; set; }
	public float Force { get; set; }
	public float AddTempWeight { get; set; }
	public float Lifetime { get; set; }
	public int NumPiercing { get; set; }
	public int NumHits { get; private set; }
	public float CriticalChance { get; set; }
	public float CriticalMultiplier { get; set; }
	public float FireIgniteChance { get; set; }

	public List<Thing> _hitThings = new List<Thing>();

	public override void Spawn()
	{
		base.Spawn();

		if(Host.IsServer)
        {
			SpriteTexture = "textures/sprites/bullet.png";

			Scale = new Vector2(0.1f, 0.1f);
			SpawnTime = 0f;
			Damage = 1f;
			AddTempWeight = 2f;
			Force = 0.75f;
			Radius = 0.1f;
			BasePivotY = -1.2f;
			HeightZ = 0f;
			//Pivot = new Vector2(0.5f, -1.2f);
			Lifetime = 1f;
			NumPiercing = 0;
			NumHits = 0;
			ShadowOpacity = 0.8f;
			ShadowScale = 0.3f;

			CollideWith.Add(typeof(Enemy));
		}

		Filter = SpriteFilter.Pixelated;
	}

    public override void ClientSpawn()
    {
        base.ClientSpawn();

		SpawnShadow(Radius * 3f);
	}

	public override void Update(float dt)
	{
		if (Game.IsGameOver)
			return;

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

		if (typeof(Enemy).IsAssignableFrom(other.GetType()))
		{
			var enemy = (Enemy)other;

			if(!enemy.IsDying && (!enemy.IsSpawning || enemy.ElapsedTime > 1.5f))
            {
				if (_hitThings.Contains(enemy))
					return;

				bool isCrit = Rand.Float(0f, 1f) < CriticalChance;
				float damage = Damage * (isCrit ? CriticalMultiplier : 1f);
				enemy.Damage(damage, Shooter, isCrit);

				enemy.Velocity += Velocity.Normal * Force;
				enemy.TempWeight += AddTempWeight;

				if (FireIgniteChance > 0f && Rand.Float(0f, 1f) < FireIgniteChance)
				{
					BurningEnemyStatus burning = (BurningEnemyStatus)enemy.AddEnemyStatus(TypeLibrary.GetDescription(typeof(BurningEnemyStatus)));
					burning.Player = Shooter;
					burning.Damage = Shooter.FireDamage * Shooter.GetDamageMultiplier();
					burning.Lifetime = Shooter.FireLifetime;
					burning.SpreadChance = Shooter.FireSpreadChance;
				}

				NumHits++;

				if (NumHits > NumPiercing)
				{
					Remove();
					return;
				}
				else
				{
					_hitThings.Add(enemy);
				}
			}
		}
	}
}
