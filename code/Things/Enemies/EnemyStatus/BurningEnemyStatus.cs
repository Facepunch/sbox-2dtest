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
	public Fire FireSprite { get; private set; }

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

		FireSprite = new Fire();
		UpdateFire();
	}

	public override void Update(float dt)
    {
		if (Enemy == null || !Enemy.IsValid)
			return;

		//DebugOverlay.Text(ElapsedTime + " / " + Lifetime, Enemy.Position, 0f, float.MaxValue);
		UpdateFire();

		if (ElapsedTime > Lifetime)
			Enemy.RemoveEnemyStatus(TypeLibrary.GetDescription(this.GetType()));

		if(_sinceDamageTime > DAMAGE_INTERVAL)
        {
			Enemy.Damage(Damage, Player);
			_sinceDamageTime = 0f;
        }
    }

	void UpdateFire()
    {
		FireSprite.Position = Enemy.Position + new Vector2(0f, 0.4f);

		bool flip = Utils.FastSin(Time.Now * 4f) < 0f;
		FireSprite.Scale = new Vector2((1f + Utils.FastSin(Time.Now * 24f) * 0.1f) * (flip ? -1f : 1f), 1f + Utils.FastSin(ElapsedTime * 14f) * 0.075f);
		FireSprite.Depth = Enemy.Depth + 2f;
		FireSprite.Opacity = (0.4f + Utils.FastSin(ElapsedTime * 20f) * 0.3f) * Utils.Map(Enemy.DeathProgress, 0f, 1f, 1f, 0f) * Utils.Map(ElapsedTime, Lifetime - 0.25f, Lifetime, 1f, 0f);
	}

	public override void Remove()
    {
		FireSprite.Delete();
	}

	public override void Refresh()
    {
		ElapsedTime = 0f;
	}

	public override void Colliding(Thing other, float percent, float dt)
	{
		bool didDamage = false;

		if (other is Enemy enemy && !enemy.IsDying && !enemy.HasEnemyStatus(TypeLibrary.GetDescription(this.GetType())))
		{
			if (_damageOtherTime > DAMAGE_INTERVAL)
			{
				enemy.Damage(Damage, Player);

				if(!enemy.HasEnemyStatus(TypeLibrary.GetDescription(typeof(BurningEnemyStatus))) && Rand.Float(0f, 1f) < SpreadChance)
                {
					BurningEnemyStatus burning = (BurningEnemyStatus)enemy.AddEnemyStatus(TypeLibrary.GetDescription(typeof(BurningEnemyStatus)));
					burning.Player = Player;
					burning.Damage = Damage;
					burning.Lifetime = Lifetime;
					burning.SpreadChance = SpreadChance;
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

public partial class Fire : Sprite
{
	public override void Spawn()
	{
		base.Spawn();

		SpriteTexture = SpriteTexture.Atlas("textures/sprites/fire_spritesheet.png", 1, 4);
		AnimationPath = "textures/sprites/fire.frames";
		AnimationSpeed = Rand.Float(3f, 6f);

		ColorTint = new Color(1f, 1f, 1f, 1f);
		Filter = SpriteFilter.Pixelated;
	}
}
