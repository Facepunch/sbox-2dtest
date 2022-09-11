using Sandbox;
using System;
using System.Diagnostics;
using System.Linq;

namespace Sandbox;

public partial class PlayerCitizen : Sprite
{
	public Vector2 MouseOffset { get; private set; }

	//private Mummy _mummy;

	public bool IsAlive { get; private set; }

	public float FeetOffset { get; private set; }

	public override void Spawn()
	{
		base.Spawn();

		//TexturePath = "textures/sprites/head.png";
		TexturePath = "textures/sprites/citizen.png";

		//Scale = new Vector2(1f, 142f / 153f);
		//Scale = new Vector2(1f, 35f / 16f) * 0.5f;

		//if (Host.IsServer)
		//{
		//	_mummy = new Mummy();
		//	_mummy.Parent = this;
		//	_mummy.LocalPosition = new Vector3(0.3f, 0f, 0f);
		//	_mummy.Depth = 1.0f;
		//}

		Health = 100f;
		IsAlive = true;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		
		Position += new Vector2( -Input.Left, Input.Forward ) * 2f * Time.Delta;
		Depth = -(Position.y + FeetOffset);

		if (MathF.Abs(Input.Left) > 0f)
			Scale = new Vector2(1f * Input.Left < 0f ? -1f : 1f, 1f) * 1f;

		//Rotation = (MathF.Atan2(MouseOffset.y, MouseOffset.x) * (180f / MathF.PI)) - 90f;
		//Scale = new Vector2( MathF.Sin( Time.Now * 4f ) * 1f + 2f, MathF.Sin( Time.Now * 3f ) * 1f + 2f );

		//DebugOverlay.Text(Position.y.ToString() + ", " + Depth.ToString(), Position);

		//DebugOverlay.Text(Position.ToString() + "\n" + Game.GetGridSquareForPos(Position).ToString(), Position + new Vector2(0.2f, 0f));
		//DebugOverlay.Line(Position, Position + new Vector2(0.01f, 0.01f), 0f, false);

		if (Host.IsServer)
        {
			//_mummy.LocalRotation += Time.Delta * 120f;

			if (Input.Pressed(InputButton.Jump) || Input.Pressed(InputButton.PrimaryAttack))
			{
				//var mummy = new Mummy
				//{
				//	Position = Position + MouseOffset,
				//	Depth = Rand.Float(-128f, 128f)
				//};

				var bullet = new Bullet
				{
					Position = Position,
					Depth = -1f,
					Velocity = Forward * 10f,
					Shooter = this
				};
			}
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
		if (ConsoleSystem.Caller.Pawn is PlayerCitizen p)
        {
			p.MouseOffset = offset;
        }
    }

	public void Damage(float damage)
    {
		Health -= damage;

		if(Health <= 0f)
        {
			Scale = new Vector2(2f, 1f);
		}
    }
}
