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

	public List<Type> CollideWith = new List<Type>();

	public float HitboxOffset { get; protected set; }

	public Vector2 HitboxPos
	{
		get { return Position + new Vector2(0f, HitboxOffset); }
		set { Position = value - new Vector2(0f, HitboxOffset); }
	}

	public Thing()
    {

    }

	public override void Spawn()
	{
		base.Spawn();
	}

	public virtual void Update(float dt)
    {
        //Utils.DrawCircle(HitboxPos, 2.5f, 8, Time.Now, Color.Red);
		//DebugText(Radius.ToString("#.#"));
	}

	public virtual void Colliding(Thing other, float percent, float dt)
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
		DebugOverlay.Text(text, Position + new Vector2(0.1f, -0.1f), 0f, float.MaxValue);
    }
}
