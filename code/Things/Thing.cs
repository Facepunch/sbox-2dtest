using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.MyGame;

namespace Sandbox;

public partial class Thing : Sprite
{
	public float Radius { get; protected set; }
	public float TempWeight { get; set; }
	public GridSquare GridPos { get; set; }
	public bool IsRemoved { get; private set; }

	public Thing()
    {

    }

	public override void Spawn()
	{
		base.Spawn();
	}

	public virtual void Update(float dt)
    {
        Utils.DrawCircle(Position, Radius, 7, Time.Now, Color.Red);
		DebugText(Depth.ToString("#.#"));
	}

	public virtual void Collide(Thing other, float percent, float dt)
    {

    }

	public void Remove()
	{
		IsRemoved = true;
		Game.RemoveThing(this);
		Delete();
	}

	public void DebugText(string text)
    {
		DebugOverlay.Text(text, Position, 0f, float.MaxValue);
    }
}
