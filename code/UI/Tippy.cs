
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Sandbox;

[UseTemplate]
internal class Tippy : Panel
{

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

	public Tippy WithContent( string title, string description )
	{
		Label titleLabel = new Label();
		titleLabel.AddClass("tippy_title");
		titleLabel.Text = title;
		AddChild(titleLabel);

		Label descriptionLabel = new Label();
		descriptionLabel.AddClass("tippy_description");
		descriptionLabel.Text = description;
		AddChild(descriptionLabel);

		return this;
	}

	public override void OnLayout( ref Rect layoutRect )
	{
		base.OnLayout( ref layoutRect );

		RemoveClass( "unset" );

		var hud = Local.Hud.Box.Rect;
		if ( !hud.IsInside( layoutRect, true ) )
		{
			SetPosition( Pivots.TopLeft );
		}
	}

	private void SetPosition( Pivots pivot )
	{
		var scale = Local.Hud.ScaleFromScreen;
		var hudsize = Local.Hud.Box.Rect.Size;
		var r = Target.Box.RectOuter;

		switch ( pivot )
		{
			case Pivots.TopRight:
				Style.Right = null;
				Style.Bottom = null;
				Style.Left = r.right * scale;
				Style.Top = r.top * scale;
				break;
			case Pivots.TopLeft:
				Style.Left = null;
				Style.Bottom = null;
				Style.Right = (hudsize.x - r.left) * scale;
				Style.Top = r.top * scale;
				break;
			case Pivots.BottomRight:
				Style.Right = null;
				Style.Top = null;
				Style.Left = r.right * scale;
				Style.Bottom = (hudsize.y - r.bottom) * scale;
				break;
			case Pivots.BottomLeft:
				Style.Left = null;
				Style.Top = null;
				Style.Right = (hudsize.x - r.left) * scale;
				Style.Bottom = (hudsize.y - r.bottom) * scale;
				break;
		}
	}

	public static Tippy Create( Panel target, Pivots pivot )
	{
		if ( Local.Hud == null ) throw new System.Exception( "Hud null" );

		var result = new Tippy();
		result.Parent = Local.Hud;
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
