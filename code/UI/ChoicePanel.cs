using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Sandbox;

public class ChoicePanel : Panel
{
	//private List<StatusIcon> _statusIcons = new List<StatusIcon>();

	public ChoicePanel()
	{
		var icon = new ChoiceModal(this);
		icon.AddClass("choice_modal");
		AddChild(icon);
	}

	public void OnChoiceMade(int num)
	{
		Log.Info("choice made: " + num);
		Delete();
	}
}

public class ChoiceModal : Panel
{
	public ChoicePanel ChoicePanel { get; set; }

	public ChoiceModal(ChoicePanel choicePanel)
    {
		ChoicePanel = choicePanel;

		//var player = MyGame.Current.LocalPlayer;



		//Log.Info("--------");
		//      Log.Info("type: " + TypeLibrary.GetDescription("ChoiceButton").TargetType);
		//Log.Info("1. " + TypeLibrary.Create("b0", TypeLibrary.GetDescription("ChoiceButton").TargetType));
		//Log.Info("2. " + (TypeLibrary.Create("b0", TypeLibrary.GetDescription("ChoiceButton").TargetType) as ChoiceButton));
		//var button0 = TypeLibrary.Create("b0", TypeLibrary.GetDescription("ChoiceButton").TargetType) as ChoiceButton;
		//var button0 = TypeLibrary.Create<Panel>("ChoiceButton");


		//var button0 = new ChoiceButton();
		//var button0 = TypeLibrary.Create<ChoiceButton>();
		var button0 = new ChoiceButton(TypeLibrary.Create<Status>("ExampleStatus"));
		button0.AddClass("choice_button");
		button0.AddEventListener("onclick", () => ChoicePanel.OnChoiceMade(0));
		AddChild(button0);

		//var button1 = new ChoiceButton();
		var button1 = new ChoiceButton(TypeLibrary.Create<Status>("ExampleStatus"));
		button1.AddClass("choice_button");
		button1.AddEventListener("onclick", () => ChoicePanel.OnChoiceMade(1));
		AddChild(button1);

		//var button2 = new ChoiceButton();
		var button2 = new ChoiceButton(TypeLibrary.Create<Status>("ExampleStatus2"));
		button2.AddClass("choice_button");
		button2.AddEventListener("onclick", () => ChoicePanel.OnChoiceMade(2));
		AddChild(button2);
	}
}

public class ChoiceButton : Panel
{
	public string Title;
	public string Description;

    public ChoiceButton(Status status)
	{
		Label titleLabel = new Label();
		titleLabel.AddClass("choice_title");
		titleLabel.Text = status.Title;
        //titleLabel.Text = "Title";
		AddChild(titleLabel);

		Image icon = new Image();
		icon.AddClass("choice_icon");
		icon.SetTexture(status.IconPath);
		AddChild(icon);

		Label descriptionLabel = new Label();
		descriptionLabel.AddClass("choice_description");
		descriptionLabel.Text = status.Description;
        //descriptionLabel.Text = "Status description goes here.";
		AddChild(descriptionLabel);
	}

 //   protected override void OnMouseOver(MousePanelEvent e)
	//{
	//	base.OnMouseOver(e);

	//	if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Description))
	//		return;

	//	Tippy.Create(this, Tippy.Pivots.TopRight).WithContent(Title, Description);
	//}
}
