using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;

namespace Sandbox
{
	public partial class Sprite : Entity
	{
		private readonly Panel _panel;
		
		[Net, Change]
		public string TexturePath { get; set; }

		[Net, Change]
		public Vector2 Size { get; set; }

		private void OnTexturePathChanged()
		{
			_panel.Style.SetBackgroundImage( TexturePath );
		}

		private void OnSizeChanged()
		{
			_panel.Style.Width = Length.Pixels( Size.x );
			_panel.Style.Height = Length.Pixels( Size.y );
		}

		public Sprite()
		{
			if ( Host.IsClient )
			{
				_panel = Canvas.Instance.AddSpritePanel();
				_panel.Style.Position = PositionMode.Absolute;
				_panel.Style.BackgroundSizeX = Length.Percent( 100f );
				_panel.Style.BackgroundSizeY = Length.Percent( 100f );
			}
		}

		public override void Spawn()
		{
			base.Spawn();

			EnableDrawing = false;

			Size = new Vector2( 128f, 128f );
		}

		[Event.Frame]
		private void ClientFrame()
		{
			_panel.Style.Left = Length.Pixels( Position.x );
			_panel.Style.Top = Length.Pixels( Position.y );
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			_panel?.Delete();
		}
	}
}
