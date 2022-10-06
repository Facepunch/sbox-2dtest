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

    public Panel HpBar { get; set; }

    public Panel HpBarOverlay { get; set; }

    public override void Tick()
    {
        base.Tick();

        if (Player == null)
            return;
        //Style.Left = 100;
        //Style.Bottom = 100;

        var name = Player.Client.Name;
        NameLabel.Text = name[..Math.Min(name.Length, 16)];

        var screenPos = MyGame.Current.MainCamera.WorldToScreen(Player.Position + new Vector2(0f, 1.33f)) * ScaleFromScreen;

        //Style.Top = Parent.MousePosition.y * ScaleToScreen;
        //Style.Left = Parent.MousePosition.x * ScaleToScreen;

        Style.Left = screenPos.x - 150;
        Style.Top = screenPos.y;

        var BAR_WIDTH = 100;
        HpBar.Style.Width = BAR_WIDTH;
        HpBarOverlay.Style.Width = BAR_WIDTH;

        var player_health_ratio = Math.Clamp(Player.Health / Player.MaxHp, 0f, 1f);

        var tr = new PanelTransform();
        tr.AddScale(new Vector3(player_health_ratio, 1f, 1f));
        HpBarOverlay.Style.Transform = tr;
        //HpBarOverlay.Style.Dirty();
        //HpBarOverlay.Style.Width = player_health_ratio * BAR_WIDTH;

        //Log.Info(screenPos.ToString());

        //Style.Left = MousePosition.x;
        //Style.Bottom = MousePosition.y;
    }
}
