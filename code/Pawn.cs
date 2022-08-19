using Sandbox;
using System;
using System.Diagnostics;
using System.Linq;

namespace Sandbox;

partial class Pawn : Sprite
{
	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		base.Spawn();

		TexturePath = "textures/sprites/chippy.png";
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		
		Position += new Vector2( -Input.Left, Input.Forward ) * 2f * Time.Delta;

		if ( IsServer )
		{
			Rotation += Time.Delta * 90f;
			Scale = new Vector2( MathF.Sin( Time.Now * 4f ) * 1f + 2f, MathF.Sin( Time.Now * 3f ) * 1f + 2f );
		}
	}

	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		MyGame.Current.MainCamera.Position = Position;
	}
}
