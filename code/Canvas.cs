using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;

namespace Sandbox
{
	public partial class Canvas : RootPanel
	{
		private static Canvas _instance;

		public static Canvas Instance => _instance ??= new Canvas();

		internal Panel Root { get; }

		public float ViewSize { get; set; } = 1024f;
		public Vector2 ViewCenter { get; set; }

		public Canvas()
		{
			Root = Add.Panel();
			Root.Style.Position = PositionMode.Relative;
			Root.Style.Overflow = OverflowMode.Visible;

			Style.Overflow = OverflowMode.Visible;
		}

		public Panel AddSpritePanel()
		{
			return Root.Add.Panel();
		}

		public override void Tick()
		{
			base.Tick();

			var screenSize = Screen.Size;

			var transform = new PanelTransform();
			
			transform.AddTranslate( Length.Pixels( screenSize.x * 0.5f - ViewCenter.x ), Length.Pixels( screenSize.y * 0.5f - ViewCenter.y ) );

			Root.Style.Transform = transform;
		}
	}
}
