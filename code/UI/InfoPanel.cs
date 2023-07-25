using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Test2D;

public class InfoPanel : Panel
{
	public DashContainer DashContainer { get; set; }

	public InfoPanel()
	{
        AddChild<XpBarPanel>( "xp_bar_panel" );

		DashContainer = new DashContainer();
		DashContainer.AddClass("info_dash_container");
		AddChild(DashContainer);
	}
}

public class DashContainer : Panel
{
	private List<DashIcon> _icons = new List<DashIcon>();

	public DashContainer()
	{
		
	}

	public override void Tick()
	{
		base.Tick();

		var player = MyGame.Current.LocalPlayer;
		if (player == null) 
			return;

		int numDashes = (int)MathF.Round(player.Stats[PlayerStat.NumDashes]);
		if (_icons.Count != numDashes)
			Refresh(numDashes);

		for(int i = 0; i < _icons.Count; i++)
        {
			var icon = _icons[i];

			var amount = i == player.NumDashesAvailable ? player.DashRechargeProgress : (i < player.NumDashesAvailable ? 1f : 0f);
			icon.OverlayPanel.Style.Width = Length.Fraction( amount );

			if (i < player.NumDashesAvailable)
            {
				icon.RemoveClass("recharge_icon");
				icon.OverlayPanel.RemoveClass("recharge");
			}
            else
            {
				icon.AddClass("recharge_icon");
				icon.OverlayPanel.AddClass("recharge");
			}
                
		}
	}

	public void Refresh(int numDashes)
	{
		foreach (var icon in _icons)
			icon.Delete();
		_icons.Clear();

		for (int i = 0; i < numDashes; i++)
        {
			var icon = new DashIcon();
			AddChild(icon);
			_icons.Add(icon);
		}
	}
}

public class DashIcon : Panel
{
	public Panel OverlayPanel { get; set; }

	public DashIcon()
	{
		AddClass("info_dash_icon");

        OverlayPanel = new Panel();
		OverlayPanel.AddClass("dash_icon_overlay");
		AddChild(OverlayPanel);

		var label = new Label();
		label.AddClass("dash_icon_label");
		label.Text = "DASH";
		AddChild(label);
	}

	protected override void OnMouseOver(MousePanelEvent e)
	{
		base.OnMouseOver(e);

		Tippy.Create(this, Tippy.Pivots.TopLeft).WithContent("Dash", "Press SPACE to use", "");
	}
}