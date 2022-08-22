using Sandbox;
using System;
using System.Diagnostics;
using System.Linq;

namespace Sandbox;

partial class Pawn : Sprite
{
	public Vector2 MousePos { get; private set; }

	public override void Spawn()
	{
		base.Spawn();

		TexturePath = "textures/sprites/head.png";

		Scale = new Vector2(1f, 1f);
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		
		Position += new Vector2( -Input.Left, Input.Forward ) * 2f * Time.Delta;

		var mouseOffset = MousePos - Position;
		Rotation = (MathF.Atan2(mouseOffset.y, mouseOffset.x) * (180f / MathF.PI)) - 90f;
		//Scale = new Vector2( MathF.Sin( Time.Now * 4f ) * 1f + 2f, MathF.Sin( Time.Now * 3f ) * 1f + 2f );

		//DebugOverlay.Text(MousePos.ToString(), Position);
		//DebugOverlay.Line(Position, MousePos, 0f, false);
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		MyGame.Current.MainCamera.Position = Position;

		MousePos = MyGame.Current.MainCamera.ScreenToWorld(MainHud.MousePos);
		SetMousePos(MousePos);
	}

	[ConCmd.Server]
	public static void SetMousePos(Vector2 pos)
    {
		if (ConsoleSystem.Caller.Pawn is Pawn p)
        {
			p.MousePos = pos;
        }
    }
}
