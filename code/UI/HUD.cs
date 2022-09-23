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

	public ToolsPanel Tools { get; private set; }

	public HUD()
	{
		//RootPanel.StyleSheet.Load("ui/HUD.scss");
  //      RootPanel.AddChild<ToolsPanel>("tools");
  //      RootPanel.AddChild<ToolsPanel>("tools");

        StyleSheet.Load("ui/HUD.scss");
        Tools = AddChild<ToolsPanel>("tools");
		Tools.Hud = this;
    }
}
