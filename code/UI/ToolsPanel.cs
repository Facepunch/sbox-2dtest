using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Sandbox;

public class ToolsPanel : Panel
{
	public HUD Hud { get; set; }

	public bool IsDirty { get; private set; }

	public ToolsPanel()
	{
		IsDirty = true;
	}

	public void Refresh()
    {
		if (!IsDirty)
			return;

		Log.Info("ToolsPanel - num statuses: " + MyGame.Current.LocalPlayer.Statuses.Count);

		// [PLAY] button
		var play = Add.Button("", "buttons");
		play.Add.Label("᱿", "play");
		play.AddEventListener("onclick", () =>
		{
			Log.Info("play");
		});

		// [NEXT] button
		var next = Add.Button("", "buttons");
		next.Add.Label("⇥", "next");
		next.AddEventListener("onclick", () =>
		{
			Log.Info("next");
		});

		// [CLEAR] button
		var clear = Add.Button("", "buttons");
		clear.Add.Label("⨯", "clear");
		clear.AddEventListener("onclick", () =>
		{
			Log.Info("clear");
		});

		IsDirty = false;
	}
}
