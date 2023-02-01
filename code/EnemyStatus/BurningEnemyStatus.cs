using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public class BurningEnemyStatus : EnemyStatus
{
	private TimeSince _sinceDamageTime;
	private const float DAMAGE_INTERVAL = 0.4f;

	public float Lifetime { get; set; }
	public float SpreadChance { get; set; }

	public PlayerCitizen Player { get; set; }

	public float Damage { get; set; }

	private TimeSince _damageOtherTime;

	public BurningEnemyStatus()
	{
		
	}

	public override void Init(Enemy enemy)
    {
		base.Init(enemy);

		Enemy.CreateBurningVfx();
	}

	public override void Update(float dt)
    {
		if (Enemy == null || !Enemy.IsValid)
			return;

		if (ElapsedTime > Lifetime)
			Enemy.RemoveEnemyStatus(this);

		if(_sinceDamageTime > DAMAGE_INTERVAL)
        {
			Enemy.DamageFire(Damage, Player);
			_sinceDamageTime = 0f;
        }
    }

	public override void Remove()
    {
		Enemy.RemoveBurningVfx();
	}

	public override void Refresh()
    {
		ElapsedTime = 0f;
	}

	public override void Colliding(Thing other, float percent, float dt)
	{
		bool didDamage = false;

		if (other is Enemy enemy && !enemy.IsDying && !enemy.HasEnemyStatus(this))
		{
			if (_damageOtherTime > DAMAGE_INTERVAL)
			{
				enemy.DamageFire(Damage, Player);

				if(!enemy.HasEnemyStatus<BurningEnemyStatus>() && Sandbox.Game.Random.Float(0f, 1f) < SpreadChance)
                {
					enemy.Burn(Player, Damage, Lifetime, SpreadChance);
					MyGame.Current.PlaySfxNearby("burn", enemy.Position, pitch: Sandbox.Game.Random.Float(1.15f, 1.35f), volume: 0.7f, maxDist: 4f);
				}

				didDamage = true;
			}
		}
		else if (other is PlayerCitizen player && !player.IsDead)
        {
			if(_damageOtherTime > DAMAGE_INTERVAL)
            {
				player.Damage(Damage);
				didDamage = true;
			}
		}

		if(didDamage)
			_damageOtherTime = 0f;
	}
}

public partial class BurningVfx : Sprite
{
	private Enemy _enemy;

	public BurningVfx(Enemy enemy)
    {
		_enemy = enemy;
    }

	public override void Spawn()
	{
		base.Spawn();

		SpriteTexture = SpriteTexture.Atlas("textures/sprites/fire_spritesheet.png", 1, 4);
		AnimationPath = "textures/sprites/fire.frames";
		AnimationSpeed = Sandbox.Game.Random.Float(3f, 6f);

		ColorTint = new Color(1f, 1f, 1f, 1f);
		Filter = SpriteFilter.Pixelated;
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		Position = _enemy.Position + new Vector2(0f, 0.4f);

        bool flip = Utils.FastSin(Time.Now * 4f) < 0f;
        Scale = new Vector2((1f + Utils.FastSin(Time.Now * 24f) * 0.1f) * (flip ? -1f : 1f), 1f + Utils.FastSin(Time.Now * 14f) * 0.075f);
        Depth = _enemy.Depth + 2f;
        Opacity = (0.4f + Utils.FastSin(Time.Now * 20f) * 0.3f) * Utils.Map(_enemy.DeathProgress, 0f, 1f, 1f, 0f);
	}
}
