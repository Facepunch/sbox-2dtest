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

	public PlayerCitizen Player { get; set; }

	private TimeSince _damagePlayerTime;

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

		//DebugOverlay.Text(Enemy.DeathProgress.ToString(), Enemy.Position, 0f, float.MaxValue);
		UpdateFire();

		if (ElapsedTime > 2f)
			Enemy.RemoveEnemyStatus(TypeLibrary.GetDescription(this.GetType()));

		if(_sinceDamageTime > 0.5f)
        {
			Enemy.Damage(2f, Player);
			_sinceDamageTime = 0f;
        }
    }

	void UpdateFire()
    {
		FireSprite.Position = Enemy.Position + new Vector2(0f, 0.4f);

		bool flip = Utils.FastSin(Time.Now * 4f) < 0f;
		FireSprite.Scale = new Vector2((1f + Utils.FastSin(Time.Now * 24f) * 0.1f) * (flip ? -1f : 1f), 1f + Utils.FastSin(ElapsedTime * 14f) * 0.075f);
		FireSprite.Depth = Enemy.Depth + 2f;
		FireSprite.Opacity = (0.4f + Utils.FastSin(ElapsedTime * 20f) * 0.3f) * Utils.Map(Enemy.DeathProgress, 0f, 1f, 1f, 0f);
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
		if (other is Enemy enemy && !enemy.IsDying && !enemy.HasEnemyStatus(TypeLibrary.GetDescription(this.GetType())))
		{
			enemy.AddEnemyStatus(TypeLibrary.GetDescription(typeof(BurningEnemyStatus)));
		}
		else if (other is PlayerCitizen player && !player.IsDead)
        {
			if(_damagePlayerTime > 0.5f)
            {
				player.Damage(5f);
				_damagePlayerTime = 0f;
			}
		}
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
