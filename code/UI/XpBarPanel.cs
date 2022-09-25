using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Sandbox;

public class XpBarPanel : Panel
{
	public Panel OverlayPanel { get; internal set; }

	public XpBarPanel()
	{
		OverlayPanel = Add.Panel("overlay_panel");
	}

	public override void Tick()
	{
		base.Tick();

		var player = MyGame.Current.LocalPlayer;
		if (player == null)
			return;

		float xp_percent = player.ExperienceCurrent / (float)player.ExperienceRequired;
		OverlayPanel.Style.Width = Length.Percent(xp_percent * 100f);
	}
}
