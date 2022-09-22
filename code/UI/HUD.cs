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

public partial class HUD : Sandbox.HudEntity<RootPanel>
{
	public static Vector2 MousePos { get; private set; }

	public HUD()
	{
		if ( !IsClient ) return;

		RootPanel.StyleSheet.Load( "ui/HUD.scss" );
		RootPanel.AddChild<ToolsPanel>( "tools" );
		RootPanel.AddChild<ToolsPanel>("tools");
	}
}
