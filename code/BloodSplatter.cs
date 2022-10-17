using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Test2D;

public partial class BloodSplatter : Sprite
{
	public override void Spawn()
	{
		base.Spawn();

		SpriteTexture = SpriteTexture.Atlas("textures/sprites/blood.png", 1, 6);
		AnimationPath = "textures/sprites/blood_splatter.frames";
		AnimationSpeed = 4f;
		Depth = -219f;
		//ColorTint = new Color(0f, 0f, 0f, 0f);
		Filter = SpriteFilter.Pixelated;
		Scale = new Vector2(1f, 1f);
    }

	[Event.Tick.Client]
	public void ClientTick()
	{
		//DebugOverlay.Text("blood", Position + new Vector2(0.1f, -0.1f), 0f, float.MaxValue);
	}
}
