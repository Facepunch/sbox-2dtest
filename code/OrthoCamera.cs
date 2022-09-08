﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public partial class OrthoCamera : CameraMode
	{
		public new float Rotation { get; set; }
		
		/// <summary>
		/// Z position in world space of the camera.
		/// The camera faces towards -Z.
		/// </summary>
		public float Depth { get; set; }

		/// <summary>
		/// World space position of the center of the view.
		/// </summary>
		public new Vector2 Position { get; set; }

		/// <summary>
		/// Height of the view in world space.
		/// </summary>
		public float Size { get; set; } = 5f;

		public OrthoCamera()
		{
			Ortho = true;
			OrthoSize = 0.01f;

			Depth = 512f;

			ZNear = 1f;
			ZFar = 1024f;
		}

		public override void Update()
		{
			Size = 10f;
			OrthoSize = Size / Screen.Height;

			base.Rotation = global::Rotation.FromYaw( 90f + Rotation ) * global::Rotation.FromPitch( 90f );
			base.Position = new Vector3( Position, Depth );
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
	}
}
