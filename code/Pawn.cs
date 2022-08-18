using Sandbox;
using System;
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

		TexturePath = "textures/sprites/mummy_walk3.png";
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Position += new Vector3( -Input.Left, -Input.Forward ) * 256f * Time.Delta;
	}

	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		// Update rotation every frame, to keep things smooth
		Rotation = Input.Rotation;
		EyeRotation = Rotation;
	}
}
