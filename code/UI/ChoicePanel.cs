using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Test2D;

public class ChoicePanel : Panel
{
	//private List<StatusIcon> _statusIcons = new List<StatusIcon>();

	public ChoicePanel()
	{
		var modal = new ChoiceModal(this);
		modal.AddClass("choice_modal");
		AddChild(modal);
	}

	public void OnChoiceMade(TypeDescription type)
	{
		//Log.Info("OnChoiceMade: " + type.TargetType.ToString());

        //ConsoleSystem.Run("add_status", type);
        PlayerCitizen.AddStatusCmd(StatusManager.TypeToIdentity(type));
		MyGame.Current.PlaySfxTarget(To.Single(Sandbox.Game.LocalClient), "click", Vector2.Zero, 0.9f, 0.75f);
		Delete();
		MyGame.Current.Hud.ChoicePanel = null;
	}

	public void Reroll()
	{
		var player = MyGame.Current.LocalPlayer;

		if (player == null || player.NumRerollAvailable <= 0)
			return;

		Delete();
		MyGame.Current.Hud.ChoicePanel = null;

		PlayerCitizen.UseRerollCmd();

		MyGame.Current.PlaySfxTarget(To.Single(Sandbox.Game.LocalClient), "reroll", player.Position, Utils.Map(player.NumRerollAvailable, 0, 20, 0.9f, 1.4f, EasingType.QuadIn), 0.6f);
	}
}

public class ChoiceModal : Panel
{
	public ChoicePanel ChoicePanel { get; set; }

	public ChoiceModal(ChoicePanel choicePanel)
    {
		ChoicePanel = choicePanel;

		var player = MyGame.Current.LocalPlayer;

		Label lvlLabel = new Label();
		lvlLabel.AddClass("choice_lvl_label");
		lvlLabel.Text = "Level " + player.Level;
		AddChild(lvlLabel);

		Panel buttonContainer = new Panel();
		buttonContainer.AddClass("choice_button_container");
		AddChild(buttonContainer);

		//Log.Info("--------");
		//      Log.Info("type: " + TypeLibrary.GetDescription("ChoiceButton").TargetType);
		//Log.Info("1. " + TypeLibrary.Create("b0", TypeLibrary.GetDescription("ChoiceButton").TargetType));
		//Log.Info("2. " + (TypeLibrary.Create("b0", TypeLibrary.GetDescription("ChoiceButton").TargetType) as ChoiceButton));
		//var button0 = TypeLibrary.Create("b0", TypeLibrary.GetDescription("ChoiceButton").TargetType) as ChoiceButton;
		//var button0 = TypeLibrary.Create<Panel>("ChoiceButton");

		//List<string> statusNames = new List<string>() { "CritChanceStatus", "CritMultiplierStatus", "ReloadSpeedStatus" };

		int numChoices = Math.Clamp((int)MathF.Round(player.NumUpgradeChoices), 1, 6);
		List<TypeDescription> types = StatusManager.GetRandomStatuses(player, numChoices);

		Style.Width = numChoices * 310f;

		//Log.Info("types: " + types);
		//Log.Info("types.Count: " + types.Count);

		for(int i = 0; i < types.Count; i++)
        {
			var type = types[i];
			var status = StatusManager.CreateStatus(type);
			var currLevel = player.GetStatusLevel (type);
			status.Level = currLevel + 1;

			var button = new ChoiceButton(status);
			button.AddClass("choice_button");
			button.AddEventListener("onclick", () => ChoicePanel.OnChoiceMade(type));
			button.status = status;
			buttonContainer.AddChild(button);
		}

		var rerollButton = Add.Panel("reroll_button");
		rerollButton.AddEventListener("onclick", () => ChoicePanel.Reroll());

		Label reroll_label = new Label();
		reroll_label.AddClass("reroll_button_label");
		reroll_label.Text = "Reroll [" + player.NumRerollAvailable + "]";
		rerollButton.AddChild(reroll_label);

		if (player.NumRerollAvailable <= 0)
        {
			rerollButton.AddClass("disabled");
			reroll_label.AddClass("disabled_text");
		}
	}
}

public class ChoiceButton : Panel
{
	public Status status { get; set; }

	//public string Title;
	//public string Description;

    public ChoiceButton(Status status)
	{
		Label titleLabel = new Label();
		titleLabel.AddClass("choice_title");
		titleLabel.Text = status.Title + (status.Level > 1 ? " [" + status.Level + "]" : "");
        //titleLabel.Text = "Title";
		AddChild(titleLabel);

		Image icon = new Image();
		icon.AddClass("choice_icon");
		icon.SetTexture(status.IconPath);
		AddChild(icon);

		Label descriptionLabel = new Label();
		descriptionLabel.AddClass("choice_description");
		descriptionLabel.Text = status.GetUpgradeDescription(status.Level);
        //descriptionLabel.Text = "Status description goes here.";
		AddChild(descriptionLabel);
	}

    //protected override void OnMouseOver(MousePanelEvent e)
    //{
    //    base.OnMouseOver(e);

    //    if (status == null || string.IsNullOrEmpty(status.Title) || string.IsNullOrEmpty(status.Description))
    //        return;

    //    Tippy.Create(this, Tippy.Pivots.TopRight).WithContent(status.Title, status.Description);
    //}
}
