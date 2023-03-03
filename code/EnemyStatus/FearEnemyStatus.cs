using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public class FearEnemyStatus : EnemyStatus
{
	public float Lifetime { get; set; }
	public float TimeScale { get; set; }

	public PlayerCitizen Player { get; set; }

	public TimeSince TimeSincePain { get; set; }
    public float PainPercent { get; set; }

    public override void Init(Enemy enemy)
    {
		base.Init(enemy);

		enemy.CreateFearVfx();
		enemy.IsFeared = true;
		TimeScale = float.MaxValue;
		TimeSincePain = 0f;
	}

	public void SetLifetime(float lifetime)
    {
		if(lifetime > Lifetime)
        {
			Lifetime = lifetime;
        }
    }

	public override void Update(float dt)
    {
		if (Enemy == null || !Enemy.IsValid)
			return;

		if (ElapsedTime > Lifetime)
			Enemy.RemoveEnemyStatus(this);

		if(TimeSincePain > 1f)
		{
			TimeSincePain = 0f;

			if(PainPercent > 0f)
			{
				float damage = PainPercent * Enemy.Health;
				Enemy.Damage(damage, null);
			}
		}
    }

	public override void StartDying()
	{
		Enemy.IsFeared = false;
	}

	public override void Remove()
    {
		Enemy.IsFeared = false;

		Enemy.RemoveFearVfx();
	}

	public override void Refresh()
    {
		ElapsedTime = 0f;
	}
}

public partial class FearVfx : Sprite
{
	private Enemy _enemy;

	public FearVfx(Enemy enemy)
	{
		_enemy = enemy;
	}

	public override void Spawn()
	{
		base.Spawn();

		SpriteTexture = SpriteTexture.Atlas("textures/sprites/fear.png", 1, 5);
        AnimationPath = "textures/sprites/fear.frames";
		AnimationSpeed = Sandbox.Game.Random.Float(2f, 2.5f);

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
		Opacity = 1f * Utils.Map(_enemy.DeathProgress, 0f, 1f, 1f, 0f);
	}
}
