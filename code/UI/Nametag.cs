using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Sandbox;

[UseTemplate]
public partial class Nametag : Panel
{
    public PlayerCitizen Player { get; set; }

    public Label NameLabel { get; set; }

    public override void Tick()
    {
        base.Tick();

        if (Player == null)
            return;
        //Style.Left = 100;
        //Style.Bottom = 100;

        NameLabel.Text = Player.Client.Name;

        var screenPos = MyGame.Current.MainCamera.WorldToScreen(Player.Position);

        //Style.Top = Parent.MousePosition.y * ScaleToScreen;
        //Style.Left = Parent.MousePosition.x * ScaleToScreen;

        Style.Left = screenPos.x;
        Style.Top = screenPos.y;

        //Log.Info(screenPos.ToString());

        //Style.Left = MousePosition.x;
        //Style.Bottom = MousePosition.y;
    }
}
