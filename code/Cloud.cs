using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Test2D;

public partial class Cloud : Sprite
{
	private TimeSince _spawnTime;
	public float Lifetime { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SpriteTexture = "textures/sprites/cloud.png";
		//AnimationPath = String.Format("textures/sprites/blood_splatter_{0}.frames", Rand.Int(0, 3).ToString()) ;
		//AnimationSpeed = Rand.Float(2f, 3.5f);
		ColorTint = new Color(1f, 1f, 1f, 0.15f);
        Filter = SpriteFilter.Pixelated;
		Scale = new Vector2((Rand.Float(0f, 1f) > 0.5f ? -1f : 1f) * Rand.Float(0.7f, 0.9f), Rand.Float(0.6f, 0.8f));
		_spawnTime = 0f;
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		Position += Velocity * Time.Delta;
		Velocity *= (1f - Time.Delta * 1.5f);
		Depth = -Position.y * 10f;

		Opacity = Utils.Map(_spawnTime, 0f, Lifetime, 1f, 0f, EasingType.QuadOut);
        if (_spawnTime > Lifetime)
        {
            Game.RemoveCloud(this);
            Delete();
        }
    }
}
