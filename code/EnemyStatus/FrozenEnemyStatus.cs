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

	public PlayerCitizen Player { get; set; }

	public override void Init(Enemy enemy)
    {
		base.Init(enemy);

		enemy.CreateFrozenVfx();
	}

	public override void Update(float dt)
    {
		if (Enemy == null || !Enemy.IsValid)
			return;

		//DebugOverlay.Text(ElapsedTime + " / " + Lifetime, Enemy.Position, 0f, float.MaxValue);

		if (ElapsedTime > Lifetime)
			Enemy.RemoveEnemyStatus(TypeLibrary.GetDescription(this.GetType()));

		Enemy.Velocity *= (1f - 15f * dt);
    }

	public override void Remove()
    {
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

		//SpriteTexture = SpriteTexture.Atlas("textures/sprites/frozenb.png", 2, 3);
		SpriteTexture = SpriteTexture.Atlas("textures/sprites/frozen.png", 1, 5);
        AnimationPath = "textures/sprites/frozen.frames";
		AnimationSpeed = Rand.Float(3f, 4f);

		Scale = new Vector2(Rand.Float(0f, 1f) < 0.5f ? -1f : 1f, 1f) * Rand.Float(0.9f, 1f);

		ColorTint = new Color(1f, 1f, 1f, 1f);
		Filter = SpriteFilter.Pixelated;
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		Position = _enemy.Position + new Vector2(0f, 0.4f);
		Depth = _enemy.Depth + 2f;
		Opacity = (0.8f + Utils.FastSin(Time.Now * 20f) * 0.2f) * Utils.Map(_enemy.DeathProgress, 0f, 1f, 1f, 0f);
	}
}
