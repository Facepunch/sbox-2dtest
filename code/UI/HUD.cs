using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Test2D;

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

	public StatusPanel StatusPanel { get; private set; }
	public InfoPanel InfoPanel { get; private set; }
	public XpBarPanel XpBarPanel { get; private set; }
	public ChoicePanel ChoicePanel { get; set; }

	public HUD()
	{
		Local.Hud = this;

		//RootPanel.StyleSheet.Load("ui/HUD.scss");
		//      RootPanel.AddChild<ToolsPanel>("tools");
		//      RootPanel.AddChild<ToolsPanel>("tools");

		StyleSheet.Load("ui/HUD.scss");

		StatusPanel = AddChild<StatusPanel>("status_panel");
		InfoPanel = AddChild<InfoPanel>("info_panel");
		XpBarPanel = AddChild<XpBarPanel>("xp_bar_panel");
		//var modal = AddChild<Modal>("modal");

		//AddChild<PlayerCursor>("cursor");
	}

	public void SpawnChoicePanel()
    {
		ChoicePanel = AddChild<ChoicePanel>("choice_panel_root");
	}

	public Nametag SpawnNametag(PlayerCitizen player)
    {
		var nametag = AddChild<Nametag>();
		nametag.Player = player;
		return nametag;
	}
}
