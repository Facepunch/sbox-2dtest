using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Test2D;

public class StatusPanel : Panel
{
	private List<StatusIcon> _statusIcons = new List<StatusIcon>();

	public StatusPanel()
	{
		
	}

	public void Refresh()
    {
		var statuses = MyGame.Current.LocalPlayer.Statuses;
		//Log.Info("StatusPanel Refresh() - num statuses: " + statuses.Count);

		for(int i = _statusIcons.Count - 1; i >= 0; i--)
        {
			var icon = _statusIcons[i];
			icon.Delete();
        }
		_statusIcons.Clear();

		foreach(KeyValuePair<int, Status> pair in statuses)
        {
			var icon = new StatusIcon();
			icon.AddClass("status_panel");
			icon.AddClass("status_icon");

			var status = pair.Value;

			icon.Style.SetBackgroundImage(status.IconPath);
			//icon.Add.Image("/textures/icons/bullets.png");
			//icon.Style.Set("background-image: url(textures/icons/bullets.png);");
			//icon.Add.Label("᱿", "play");
			AddChild(icon);
			
			icon.Title = status.Title;
			icon.Description = status.Description;
			icon.Level = status.Level;
			icon.MaxLevel = status.MaxLevel;
			icon.LevelLabel.Text = status.Level > 1 ? status.Level.ToString() : "";
			icon.LevelLabelBg.Text = status.Level > 1 ? status.Level.ToString() : "";

			//icon.AddEventListener("onclick", () =>
			//{
			//	Log.Info("play");
			//});

			_statusIcons.Add(icon);
		}
	}
}

public class StatusIcon : Panel
{
	public string Title;
	public string Description;

	public int Level { get; set; }
	public int MaxLevel { get; set; }
	public Label LevelLabelBg;
	public Label LevelLabel;

	public StatusIcon()
    {
		LevelLabel = new Label();
		LevelLabel.AddClass("status_level_label");
		AddChild(LevelLabel);

		LevelLabelBg = new Label();
		LevelLabelBg.AddClass("status_level_label_bg");
		LevelLabel.AddChild(LevelLabelBg);
	}

	protected override void OnMouseOver(MousePanelEvent e)
	{
		base.OnMouseOver(e);

		if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Description))
			return;

		var levelText = Level == MaxLevel
			? "MAX"
			: Level + "/" + MaxLevel;

		Tippy.Create(this, Tippy.Pivots.TopRight).WithContent(Title, Description, levelText);
	}

    public override void OnMouseWheel(float value)
    {
        base.OnMouseWheel(value);

		Parent.OnMouseWheel(value);
    }
}
