using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Test2D;

public class DeathPanel : Panel
{
	public DeathPanel()
	{
		var icon = new DeathModal(this);
		icon.AddClass("death_panel");
		AddChild(icon);
	}

	public void OnDeathButtonClicked()
	{
		MyGame.RestartCmd();
		Delete();
	}
}

public class DeathModal : Panel
{
	public DeathPanel DeathPanel { get; set; }

	public DeathModal(DeathPanel deathPanel)
    {
		DeathPanel = deathPanel;

		Label label = new Label();
		label.AddClass("death_label");
		label.Text = "You're Dead...";
		//titleLabel.Text = "Title";
		AddChild(label);

		var button = Add.Panel("death_button");
		button.AddEventListener("onclick", () => DeathPanel.OnDeathButtonClicked());

		Label button_label = new Label();
		button_label.AddClass("death_button_label");
		button_label.Text = "Retry";
		button.AddChild(button_label);
	}
}
