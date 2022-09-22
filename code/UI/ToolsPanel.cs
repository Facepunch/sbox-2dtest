using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Sandbox;

public class ToolsPanel : Panel
{
	public ToolsPanel()
	{
		// [PLAY] button
		var play = Add.Button( "", "buttons" );
		play.Add.Label("᱿", "play");
		play.AddEventListener( "onclick", () =>
		{
			Log.Info("play");
		} );

		// [NEXT] button
		var next = Add.Button( "", "buttons" );
		next.Add.Label( "⇥", "next" );
		next.AddEventListener( "onclick", () =>
		{
			Log.Info("next");
		} );

		// [CLEAR] button
		var clear = Add.Button( "", "buttons" );
		clear.Add.Label( "⨯", "clear" );
		clear.AddEventListener( "onclick", () =>
		{
			Log.Info("clear");
		} );
	}
}
