using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public partial class OrthoCamera : CameraMode
	{
		public new float Rotation { get; set; }

		public float Depth { get; set; }

		public Vector2 Position { get; set; }

		public OrthoCamera()
		{
			Ortho = true;
			OrthoSize = 1f;

			Depth = 512f;

			ZNear = 1f;
			ZFar = 1024f;
		}

		public override void Update()
		{
			base.Rotation = global::Rotation.FromYaw( 90f + Rotation ) * global::Rotation.FromPitch( 90f );
			base.Position = new Vector3( Position, Depth );
		}
	}
}
