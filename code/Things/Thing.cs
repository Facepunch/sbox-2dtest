using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class Thing : Sprite
{
	[Net] public float Radius { get; set; }
	public float TempWeight { get; set; }
	public GridSquare GridPos { get; set; }
	public bool IsRemoved { get; private set; }

	public List<Type> CollideWith = new List<Type>();

	[Net] public float ShadowOpacity { get; set; }
	public Shadow Shadow { get; set; } // client only 


	public Thing()
    {

    }

	public override void Spawn()
	{
		base.Spawn();
	}

	public virtual void Update(float dt)
    {
		//Utils.DrawCircle(Position, Radius, 8, Time.Now, Color.Red);
		//DebugText(Radius.ToString("#.#"));
		//DebugText(Depth.ToString("#.#"));
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

	protected void SpawnShadow(float size)
    {
		Shadow = new Shadow();
		Shadow.SetThing(this);
		Shadow.Scale = size;
	}
}
