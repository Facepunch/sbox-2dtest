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
	[Net] public float ShadowScale { get; set; }
	public Shadow Shadow { get; set; } // client only 
	public float BasePivotY { get; set; }

	public float TimeScale { get; set; }

	float _heightZ = 0.0f;
	public float HeightZ { set { _heightZ = value; Pivot = new Vector2(0.5f, BasePivotY - _heightZ); } get { return _heightZ; } }

	public Thing()
    {
		TimeScale = 1f;
    }

	public override void Spawn()
	{
		base.Spawn();
	}

	public virtual void Update(float dt)
    {
		//Utils.DrawCircle(Position, Radius, 8, Time.Now, Color.Red);
		//DebugText(Radius.ToString("#.#"));
		//DebugText("Depth: " + Depth);
		//DebugText("ColorFill: " + ColorFill.ToString());
	}

	public virtual void Colliding(Thing other, float percent, float dt)
    {

    }

	public virtual void Remove()
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

	[ClientRpc]
	public void SpawnCloudClient(Vector2 pos, Vector2 vel)
	{
		var cloud = Game.SpawnCloud(Position);
		cloud.Velocity = vel;
	}
}
