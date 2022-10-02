using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Sandbox;

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

		foreach(var status in statuses)
        {
			var icon = new StatusIcon();
			icon.AddClass("status_panel");
			icon.AddClass("status_icon");

			icon.Style.SetBackgroundImage(status.IconPath);
			//icon.Add.Image("/textures/icons/bullets.png");
			//icon.Style.Set("background-image: url(textures/icons/bullets.png);");
			//icon.Add.Label("᱿", "play");
			AddChild(icon);
			
			icon.Title = status.Title;
			icon.Description = status.Description;

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

	protected override void OnMouseOver(MousePanelEvent e)
	{
		base.OnMouseOver(e);

		if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Description))
			return;

		Tippy.Create(this, Tippy.Pivots.TopRight).WithContent(Title, Description);
	}

    public override void OnMouseWheel(float value)
    {
        base.OnMouseWheel(value);

		Parent.OnMouseWheel(value);
    }
}
