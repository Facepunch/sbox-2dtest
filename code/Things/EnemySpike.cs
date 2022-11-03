using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;
public partial class EnemySpike : Thing
{
	public TimeSince SpawnTime { get; private set; }

	public Enemy Shooter { get; set; }

	public float Damage { get; set; }
	public float Lifetime { get; set; }

	public SpikeBackground Background { get; set; }

	public List<Thing> _hitThings = new List<Thing>();

	public override void Spawn()
	{
		base.Spawn();

		if(Host.IsServer)
        {
			SpriteTexture = SpriteTexture.Atlas("textures/sprites/spike_top.png", 3, 3);
			AnimationPath = "textures/sprites/spike.frames";
			AnimationSpeed = 3f;

			Scale = new Vector2(Rand.Float(0f, 1f) < 0.5f ? -1f : 1f, 1f) * 1.3f;
			SpawnTime = 0f;
			Damage = 10f;
			Radius = 0.3f;
			BasePivotY = 0.5f;
            HeightZ = 0f;
			//Pivot = new Vector2(0.5f, -0.9f);

			ShadowOpacity = 0.8f;
			ShadowScale = 0.6f;
			ColorTint = Color.Red;
			Lifetime = 2.1f;

			CollideWith.Add(typeof(PlayerCitizen));
		}

		Filter = SpriteFilter.Pixelated;
	}

    public override void ClientSpawn()
    {
        base.ClientSpawn();

		Background = new SpikeBackground();
		Background.Position = Position;
	}

    public override void Update(float dt)
	{
		if (Game.IsGameOver)
			return;

		base.Update(dt);

		//DebugText(SpawnTime.ToString());
        //Utils.DrawCircle(Position, Radius, 8, Time.Now, Color.Red);

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

		if (SpawnTime < 1.25f || SpawnTime > 1.6f)
			return;

		if (typeof(PlayerCitizen).IsAssignableFrom(other.GetType()))
		{
			var player = (PlayerCitizen)other;
			if (!player.IsDead)
            {
				if (_hitThings.Contains(player))
					return;

				player.Damage(Damage);
				//player.Velocity += Direction * 2f;
				Game.PlaySfxNearby("splash", Position, pitch: Rand.Float(0.95f, 1.05f), volume: 1f, maxDist: 4f);

				_hitThings.Add(player);
			}
		}
	}

	public override void Remove()
	{
		RemoveClient();
		base.Remove();
	}

	[ClientRpc]
	public void RemoveClient()
	{
		Background.Delete();
	}
}

public partial class SpikeBackground : Sprite
{
	public Thing Thing { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SpriteTexture = SpriteTexture.Atlas("textures/sprites/spike_bottom.png", 3, 3);
		AnimationPath = "textures/sprites/spike.frames";
		AnimationSpeed = 3f;

		Depth = -217f;
		Filter = SpriteFilter.Pixelated;
		Scale = new Vector2(Rand.Float(0f, 1f) < 0.5f ? -1f : 1f, 1f) * 1.3f;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

	}

	//public void SetThing(Thing thing)
	//{
	//	Thing = thing;
	//	Filter = SpriteFilter.Pixelated;
	//}

	//[Event.Tick.Client]
	//public void ClientTick()
	//{
	//	if (Thing == null || !Thing.IsValid())
	//	{
	//		Delete();
	//		return;
	//	}

	//	Position = Thing.Position;
	//	ColorTint = new Color(0f, 0f, 0f, Thing.ShadowOpacity);
	//	Scale = Thing.ShadowScale;

	//	//DebugOverlay.Text("ColorFill: " + ColorFill.ToString(), Position + new Vector2(0.1f, -0.1f), 0f, float.MaxValue);
	//}
}

