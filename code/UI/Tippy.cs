
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Test2D;

[UseTemplate]
internal class Tippy : Panel
{
	public Label TitleLabel { get; set; }
	public Label LevelLabel { get; set; }
	public Label DescLabel { get; set; }


	private Panel Target;
	private Pivots Pivot;

	//public Panel Canvas { get; set; }

	public override void Tick()
	{
		base.Tick();

		if( Target == null
			|| Target.Parent == null
			|| !Target.HasHovered )
		{
			Delete();
		}
	}

	//public Tippy WithMessage(string title, string description) => WithContent(
	//	Add.Label(title),
	//	Add.Label(description)
	//);

	public Tippy WithContent( string title, string description, string level )
	{
		TitleLabel.Text = title;
		LevelLabel.Text = level;
		DescLabel.Text = description;

		return this;
	}

	public override void OnLayout( ref Rect layoutRect )
	{
		base.OnLayout( ref layoutRect );

		RemoveClass( "unset" );

		var hud = Sandbox.Game.RootPanel.Box.Rect;
		if ( !hud.IsInside( layoutRect, true ) )
		{
			SetPosition( Pivots.TopLeft );
		}
	}

	private void SetPosition( Pivots pivot )
	{
		var scale = Sandbox.Game.RootPanel.ScaleFromScreen;
		var hudsize = Sandbox.Game.RootPanel.Box.Rect.Size;
		var r = Target.Box.RectOuter;

		switch ( pivot )
		{
			case Pivots.TopRight:
				Style.Right = null;
				Style.Bottom = null;
				Style.Left = r.Right * scale;
				Style.Top = r.Top * scale;
				break;
			case Pivots.TopLeft:
				Style.Left = null;
				Style.Bottom = null;
				Style.Right = (hudsize.x - r.Left) * scale;
				Style.Top = r.Top * scale;
				break;
			case Pivots.BottomRight:
				Style.Right = null;
				Style.Top = null;
				Style.Left = r.Right * scale;
				Style.Bottom = (hudsize.y - r.Bottom) * scale;
				break;
			case Pivots.BottomLeft:
				Style.Left = null;
				Style.Top = null;
				Style.Right = (hudsize.x - r.Left) * scale;
				Style.Bottom = (hudsize.y - r.Bottom) * scale;
				break;
		}
	}

	public static Tippy Create( Panel target, Pivots pivot )
	{
		if ( Sandbox.Game.RootPanel == null ) throw new System.Exception( "Hud null" );

		var result = new Tippy();
		result.Parent = Sandbox.Game.RootPanel;
		result.Pivot = pivot;
		result.Target = target;
		result.AddClass( "unset" );
		result.SetPosition( pivot );

		return result;
	}

	public enum Pivots
	{
		TopLeft,
		TopRight,
		BottomRight,
		BottomLeft
	}

}
