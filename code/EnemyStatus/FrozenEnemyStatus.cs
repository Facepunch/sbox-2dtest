using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public class FrozenEnemyStatus : EnemyStatus
{
	public float Lifetime { get; set; }
	public float TimeScale { get; set; }

	public PlayerCitizen Player { get; set; }

	public override void Init(Enemy enemy)
    {
		base.Init(enemy);

		enemy.CreateFrozenVfx();
		enemy.IsFrozen = true;
		TimeScale = float.MaxValue;
	}

	public void SetLifetime(float lifetime)
    {
		if(lifetime > Lifetime)
        {
			Lifetime = lifetime;
        }
    }

	public void SetTimeScale(float timeScale)
    {
		timeScale = MathF.Max(timeScale, 0.01f);

		if(timeScale < TimeScale)
		{
			Enemy.AnimSpeedModifier = timeScale;
			Enemy.TimeScale = timeScale;
		}
	}

	public override void Update(float dt)
    {
		if (Enemy == null || !Enemy.IsValid)
			return;

		if (ElapsedTime > Lifetime)
			Enemy.RemoveEnemyStatus(this);

		Enemy.Velocity *= (1f - Utils.Map(TimeScale, 0.6f, 0f, 3f, 15f) * dt);
    }

	public override void StartDying()
	{
		Enemy.AnimSpeedModifier = 1f;
		Enemy.TimeScale = 1f;
		Enemy.IsFrozen = false;

        if (Player != null && Player.Stats[PlayerStat.FrozenShardsNum] > 0f)
        {
			int maxShardsNum = (int)Player.Stats[PlayerStat.FrozenShardsNum];
			int numShards = Sandbox.Game.Random.Int(1, maxShardsNum);
			Player.SpawnBulletRing(Enemy.Position, numShards);
        }
    }

	public override void Remove()
    {
		Enemy.AnimSpeedModifier = 1f;
		Enemy.TimeScale = 1f;
		Enemy.IsFrozen = false;

		Enemy.RemoveFrozenVfx();
	}

	public override void Refresh()
    {
		ElapsedTime = 0f;
	}
}

public partial class FrozenVfx : Sprite
{
	private Enemy _enemy;

	public FrozenVfx(Enemy enemy)
	{
		_enemy = enemy;
	}

	public override void Spawn()
	{
		base.Spawn();

		SpriteTexture = SpriteTexture.Atlas("textures/sprites/frozen.png", 1, 5);
        AnimationPath = "textures/sprites/frozen.frames";
		AnimationSpeed = Sandbox.Game.Random.Float(3f, 4f);

		Scale = new Vector2(Sandbox.Game.Random.Float(0f, 1f) < 0.5f ? -1f : 1f, 1f) * Sandbox.Game.Random.Float(0.9f, 1f);

		ColorTint = new Color(1f, 1f, 1f, 1f);
		Filter = SpriteFilter.Pixelated;
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		if (!_enemy.IsValid)
			return;

		Position = _enemy.Position + new Vector2(0f, 0.4f);
		Depth = _enemy.Depth + 2f;
		Opacity = (0.8f + Utils.FastSin(Time.Now * 20f) * 0.2f) * Utils.Map(_enemy.DeathProgress, 0f, 1f, 1f, 0f);
	}
}
