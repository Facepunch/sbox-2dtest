using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class Grenade : Thing
{
	public TimeSince SpawnTime { get; private set; }

	public float Lifetime { get; set; }
	public float Damage { get; set; }
    public float ExplosionRadius { get; set; }
	private const float BASE_EXPLOSION_MODIFIER = 0.6f;
	[Net] public float ExplosionSizeMultiplier { get; set; }
	public float Friction { get; set; }
	public PlayerCitizen Player { get; set; }
	public float StickyPercent { get; set; }
    public float FearChance { get; set; }

    public Grenade()
    {

    }

	public override void Spawn()
	{
		base.Spawn();

		if (Sandbox.Game.IsServer)
		{
            SpriteTexture = "textures/sprites/grenade.png";
			BasePivotY = 0.1f;
			HeightZ = 0f;
			//Pivot = new Vector2(0.5f, 0.225f);
			Lifetime = 3f;

			Scale = new Vector2(1f, 1f) * 0.275f;
			SpawnTime = 0f;
			Radius = 0.175f;

			CollideWith.Add(typeof(Enemy));
			CollideWith.Add(typeof(PlayerCitizen));
			CollideWith.Add(typeof(Coin));

			ShadowOpacity = 0.8f;
			ShadowScale = 0.5f;
			//SetValue(Sandbox.Game.Random.Int(1, 7));

			Damage = 25f;
			ExplosionRadius = 1.45f;
			Friction = 0.95f;
        }

		Filter = SpriteFilter.Pixelated;
	}

    public override void ClientSpawn()
    {
        base.ClientSpawn();

		SpawnShadow(0.4f);
	}

	public override void Update(float dt)
	{
		if (Game.IsGameOver)
			return;

		base.Update(dt);

		Position += Velocity * dt;

		float BUFFER = -0.175f;
        if (Position.x < Game.BOUNDS_MIN.x + BUFFER)
            Velocity = new Vector2(MathF.Abs(Velocity.x), Velocity.y);
        else if (Position.x > Game.BOUNDS_MAX.x - BUFFER)
            Velocity = new Vector2(-MathF.Abs(Velocity.x), Velocity.y);
        
		if (Position.y < Game.BOUNDS_MIN.y + BUFFER)
			Velocity = new Vector2(Velocity.x, MathF.Abs(Velocity.y));
		else if (Position.y > Game.BOUNDS_MAX.y - BUFFER)
            Velocity = new Vector2(Velocity.x, -MathF.Abs(Velocity.y));

		Velocity *= (1f - dt * Friction);

		Depth = -Position.y * 10f;

		ColorTint = Color.Lerp(StickyPercent <= 0f ? Color.Red : Color.Magenta, new Color(0f, 0.01f, 0f), 0.5f + MathF.Sin(SpawnTime.Relative * Utils.Map(SpawnTime, 0f, Lifetime, 1f, 16f, EasingType.QuadIn)) * 0.5f);
        Scale = new Vector2(1f, 1f) * 0.275f * (0.9f + MathF.Sin(SpawnTime.Relative * Utils.Map(SpawnTime, 0f, Lifetime, 1f, 16f, EasingType.QuadIn)) * 0.1f);
        
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

		if (SpawnTime > Lifetime)
		{
			Explode();
        }

		//DebugText(Value.ToString());
	}

	public override void Colliding(Thing other, float percent, float dt)
	{
		base.Colliding(other, percent, dt);

		float repelAmount = (SpawnTime < 0.1f || StickyPercent <= 0f) ? 30f : Utils.Map(StickyPercent, 0f, 1f, 10f, -40f);

        if ((other is Enemy enemy && !enemy.IsDying) || (other is PlayerCitizen player && !player.IsDead))
		{
			Velocity += (Position - other.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * repelAmount * (1f + other.TempWeight) * dt;
		}
	}

	public void Explode()
	{
		ExplodeClient();

        List<Thing> nearbyThings = new List<Thing>();

        for (int dx = -2; dx <= 2; dx++)
            for (int dy = -2; dy <= 2; dy++)
                Game.AddThingsInGridSquare(new GridSquare(GridPos.x + dx, GridPos.y + dy), nearbyThings);

        foreach (Thing thing in nearbyThings)
        {
            if (thing == this)
                continue;

			float radius = ExplosionRadius * BASE_EXPLOSION_MODIFIER * ExplosionSizeMultiplier;
			float damage = Damage * Player?.Stats[PlayerStat.ExplosionDamageMultiplier] ?? 1f;

            if (thing is Enemy enemy && !enemy.IsDying && (!enemy.IsSpawning || enemy.ElapsedTime > 0.75f))
            {
                var dist_sqr = (thing.Position - Position).LengthSquared;
                if (dist_sqr < MathF.Pow(radius, 2f))
				{
                    enemy.Damage(damage, null, false);

					if(Sandbox.Game.Random.Float(0f, 1f) < FearChance && !enemy.IsDying)
					{
                        if (!enemy.HasEnemyStatus<FearEnemyStatus>())
                            MyGame.Current.PlaySfxNearby("fear", enemy.Position, pitch: Sandbox.Game.Random.Float(0.95f, 1.05f), volume: 0.6f, maxDist: 6f);

                        enemy.Fear(Player);
                    }
                }
            }
            else if (thing is PlayerCitizen player && !player.IsDead)
            {
                var dist_sqr = (thing.Position - Position).LengthSquared;
                if (dist_sqr < MathF.Pow(radius, 2f) * 0.94f)
                    player.Damage(damage, DamageType.Explosion);
            }
        }

        Remove();
    }

    [ClientRpc]
	public void ExplodeClient()
	{
		float scaleModifier = BASE_EXPLOSION_MODIFIER * ExplosionSizeMultiplier;
        Game.SpawnExplosionEffect(Position, scaleModifier);
        Game.PlaySfxNearby("explode", Position, pitch: Sandbox.Game.Random.Float(0.9f, 1.1f), volume: 1f, maxDist: 6f);
    }
}
