using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Test2D;

public partial class Camera2D
{
	private readonly SceneCamera _inner;

	public static implicit operator SceneCamera( Camera2D camera )
	{
		return camera._inner;
	}

	public float Rotation { get; set; }
		
	/// <summary>
	/// Z position in world space of the camera.
	/// The camera faces towards -Z.
	/// </summary>
	public float Depth { get; set; }

	/// <summary>
	/// World space position of the center of the view.
	/// </summary>
	public Vector2 Position { get; set; }
	public Vector2 LowerLeftWorld { get; set; }
	public Vector2 UpperRightWorld { get; set; }
	public Vector2 WorldSize { get; set; }

	/// <summary>
	/// Height of the view in world space.
	/// </summary>
	public float Size { get; set; } = 5f;

	public float ZNear
	{
		get => _inner.ZNear;
		set => _inner.ZNear = value;
	}

	public float ZFar
	{
		get => _inner.ZFar;
		set => _inner.ZFar = value;
	}

	public Camera2D( string name = "Unnamed 2D" )
	{
		_inner = new SceneCamera( name )
		{
			Ortho = true,
			OrthoHeight = 0.01f
		};

		Depth = 512f;

		ZNear = 1f;
		ZFar = 1024f;
	}

	public void Update()
	{
		Size = 10f;

		_inner.OrthoHeight = Size / Screen.Height;

		_inner.Rotation = global::Rotation.FromYaw( 90f + Rotation ) * global::Rotation.FromPitch( 90f );
		_inner.Position = new Vector3( Position, Depth );

		LowerLeftWorld = ScreenToWorld(new Vector2(0, Screen.Height));
		UpperRightWorld = ScreenToWorld(new Vector2(Screen.Width, 0));
		WorldSize = UpperRightWorld - LowerLeftWorld;
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
