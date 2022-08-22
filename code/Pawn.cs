using Sandbox;
using System;
using System.Diagnostics;
using System.Linq;

namespace Sandbox;

partial class Pawn : Sprite
{
	public Vector2 MouseOffset { get; private set; }

	private Mummy _mummy;

	public override void Spawn()
	{
		base.Spawn();

		TexturePath = "textures/sprites/head.png";

		Scale = new Vector2(1f, 1f);

		if (Host.IsServer)
		{
			_mummy = new Mummy();
			_mummy.Parent = this;
			_mummy.LocalPosition = new Vector3(0.3f, 0f, 0f);
			_mummy.Depth = 1.0f;
		}
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		
		Position += new Vector2( -Input.Left, Input.Forward ) * 2f * Time.Delta;

		Rotation = (MathF.Atan2(MouseOffset.y, MouseOffset.x) * (180f / MathF.PI)) - 90f;
		//Scale = new Vector2( MathF.Sin( Time.Now * 4f ) * 1f + 2f, MathF.Sin( Time.Now * 3f ) * 1f + 2f );

		//DebugOverlay.Text(MousePos.ToString(), Position);
		//DebugOverlay.Line(Position, MousePos, 0f, false);

		if(Host.IsServer)
        {
			_mummy.LocalRotation += Time.Delta * 120f;
			Scale = new Vector2(1f, 142f / 153f);
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		MyGame.Current.MainCamera.Position = Position;

		MouseOffset = MyGame.Current.MainCamera.ScreenToWorld(MainHud.MousePos) - Position;
		SetMouseOffset(MouseOffset);
	}

	[ConCmd.Server]
	public static void SetMouseOffset(Vector2 offset)
    {
		if (ConsoleSystem.Caller.Pawn is Pawn p)
        {
			p.MouseOffset = offset;
        }
    }
}
