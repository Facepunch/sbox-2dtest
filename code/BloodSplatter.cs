using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Test2D;

public partial class BloodSplatter : Sprite
{
	private TimeSince _spawnTime;
	public float Lifetime { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SpriteTexture = SpriteTexture.Atlas("textures/sprites/blood.png", 1, 9);
		AnimationPath = String.Format("textures/sprites/blood_splatter_{0}.frames", Rand.Int(0, 3).ToString()) ;
		AnimationSpeed = Rand.Float(2f, 3.5f);
		Depth = -219f;
		ColorTint = Color.White;
        Filter = SpriteFilter.Pixelated;
		Scale = new Vector2((Rand.Float(0f, 1f) > 0.5f ? -1f : 1f) * Rand.Float(0.9f, 1.2f), Rand.Float(0.9f, 1.2f));
		_spawnTime = 0f;
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		//DebugOverlay.Text("blood", Position + new Vector2(0.1f, -0.1f), 0f, float.MaxValue);
		Opacity = Utils.Map(_spawnTime, 0f, Lifetime, 1f, 0f);
        if (_spawnTime > Lifetime)
        {
            Game.RemoveBloodSplatter(this);
            Delete();
        }
    }
}
