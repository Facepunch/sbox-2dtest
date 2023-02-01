using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Test2D;

public class DeathPanel : Panel
{
	public DeathModal DeathModal { get; private set; }

	public DeathPanel()
	{
		DeathModal = new DeathModal(this);
		DeathModal.AddClass("death_panel");
		AddChild(DeathModal);
	}

	public void OnDeathButtonClicked()
	{
		if (Sandbox.Game.LocalClient.IsListenServerHost)
        {
			MyGame.RestartCmd();
			Delete();
			MyGame.Current.Hud.DeathPanel = null;
		}
	}

	public void Victory()
    {
		DeathModal.TextLabel.Text = "Victory!";
    }
}

public class DeathModal : Panel
{
	public DeathPanel DeathPanel { get; set; }
	public Label TextLabel { get; set; }

	public DeathModal(DeathPanel deathPanel)
    {
		DeathPanel = deathPanel;

		TextLabel = new Label();
		TextLabel.AddClass("death_label");
		TextLabel.Text = "You're Dead...";
		//titleLabel.Text = "Title";
		AddChild(TextLabel);

		var button = Add.Panel("death_button");
		button.AddEventListener("onclick", () => DeathPanel.OnDeathButtonClicked());

		Label button_label = new Label();
		button_label.AddClass("death_button_label");
		button_label.Text = "Retry";
		button.AddChild(button_label);

		if(!Sandbox.Game.LocalClient.IsListenServerHost)
        {
			button.AddClass("disabled");
			button_label.Text += " (Host only)";
		}
	}
}
