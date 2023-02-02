using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Test2D;

public partial class Camera2D
{
	public static Camera2D Current { get; set; }

	public float Rotation { get; set; }

	/// <summary>
	/// Z position in world space of the camera.
	/// The camera faces towards -Z.
	/// </summary>
	public float Depth { get; set; } = 512f;

	/// <summary>
	/// World space position of the center of the view.
	/// </summary>
	public Vector2 Position { get; set; }
	public Vector2 LowerLeftWorld => ScreenToWorld( new Vector2( 0, Screen.Height ) );
	public Vector2 UpperRightWorld => ScreenToWorld( new Vector2( Screen.Width, 0 ) );
	public Vector2 WorldSize => UpperRightWorld - LowerLeftWorld;

	/// <summary>
	/// Height of the view in world space.
	/// </summary>
	public float Size { get; set; } = 10f;

	public float ZNear { get; set; } = 1f;

	public float ZFar { get; set; } = 1024f;

	public void Update()
	{
		if ( Current != this ) return;

		var target = Camera.Current ?? Camera.Main;
		
		target.Ortho = true;
		target.OrthoHeight = Size;
		target.OrthoWidth = Size * Screen.Width / Screen.Height;

		target.ZNear = ZNear;
		target.ZFar = ZFar;

		target.Rotation = global::Rotation.FromYaw( 90f + Rotation ) * global::Rotation.FromPitch( 90f );
		target.Position = new Vector3( Position, Depth );
	}

	public Vector2 ScreenToWorld(Vector2 screenPos)
	{
		screenPos /= Screen.Size;
		screenPos -= new Vector2(0.5f, 0.5f);
		screenPos *= Size;
		screenPos.x *= Screen.Aspect;
		screenPos.y *= -1;
		screenPos += Position;

		return screenPos;
	}

	public Vector2 WorldToScreen(Vector2 worldPos)
	{
		worldPos -= Position;
		worldPos.y *= -1;
		worldPos.x /= Screen.Aspect;
		worldPos /= Size;
		worldPos += new Vector2(0.5f, 0.5f);
		worldPos *= Screen.Size;
			
		return worldPos;
	}
}
