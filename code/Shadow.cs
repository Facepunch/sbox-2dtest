using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Test2D;

public partial class Shadow : Sprite
{
	public Thing Thing { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SpriteTexture = "textures/sprites/shadow3.png";
		Depth = -50f;
		ColorFill = new Color(0f, 0f, 0f, 1f);
		Filter = SpriteFilter.Pixelated;
		Scale = new Vector2(1f, 1f);
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();

        //SpriteTexture = "textures/sprites/shadow3.png";
        //Depth = -50f;
        //Scale = 1f;
        //ColorFill = new Color(0f, 0f, 0f);
        //Filter = SpriteFilter.Pixelated;
        //Scale = new Vector2(2f, 2f);

        //Log.Info("ClientSpawn");
    }

    public void SetThing(Thing thing)
	{
		Thing = thing;
		Filter = SpriteFilter.Pixelated;
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		if (Thing == null || !Thing.IsValid())
        {
			Delete();
			return;
		}
			
		Position = Thing.Position;

		//DebugOverlay.Text("ColorFill: " + ColorFill.ToString(), Position + new Vector2(0.1f, -0.1f), 0f, float.MaxValue);
	}
}
