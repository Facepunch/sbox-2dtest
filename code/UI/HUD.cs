using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Sandbox;

public class PlayerCursor : Panel
{

	public PlayerCursor()
	{

	}

	public override void Tick()
	{

		Style.Top = Parent.MousePosition.y * ScaleToScreen;
		Style.Left = Parent.MousePosition.x * ScaleToScreen;

	}

}

//public partial class HUD : Sandbox.HudEntity<RootPanel>
public partial class HUD : RootPanel
{
	//public static Vector2 MousePos { get; private set; }

	public ToolsPanel ToolsPanel { get; private set; }
	public InfoPanel InfoPanel { get; private set; }
	public XpBarPanel XpBarPanel { get; private set; }

	public HUD()
	{
		//RootPanel.StyleSheet.Load("ui/HUD.scss");
  //      RootPanel.AddChild<ToolsPanel>("tools");
  //      RootPanel.AddChild<ToolsPanel>("tools");

        StyleSheet.Load("ui/HUD.scss");

		ToolsPanel = AddChild<ToolsPanel>("tools");
		InfoPanel = AddChild<InfoPanel>("info_panel");
		XpBarPanel = AddChild<XpBarPanel>("xp_bar_panel");

		//AddChild<PlayerCursor>("cursor");
	}
}
