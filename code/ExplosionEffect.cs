using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Test2D;

public partial class ExplosionEffect : Sprite
{
	private TimeSince _spawnTime;
	public float Lifetime { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SpriteTexture = SpriteTexture.Atlas("textures/sprites/explosion.png", 2, 5);
		AnimationPath = "textures/sprites/explosion.frames";
		AnimationSpeed = 3f;
		Depth = 222f;
		ColorTint = Color.Red;
        Filter = SpriteFilter.Pixelated;
		Scale = new Vector2(1f, 0.95f) * 2.85f;
		_spawnTime = 0f;
		Opacity = 0.8f;
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		Opacity = Utils.Map(_spawnTime, 0f, Lifetime, 0.8f, 0f);

        if (_spawnTime > Lifetime)
        {
            Game.RemoveExplosionEffect(this);
            Delete();
        }
    }
}
