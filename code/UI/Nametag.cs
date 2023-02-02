using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Test2D;

[UseTemplate]
public partial class Nametag : Panel
{
    public PlayerCitizen Player { get; set; }

    public Label NameLabel { get; set; }

    public Panel HpBar { get; set; }
    public Panel HpBarOverlay { get; set; }
    public Panel HpBarDelta { get; set; }
    public Panel ReloadBar { get; set; }
    public Panel ReloadBarTickRight { get; set; }

    public void AddReloadBar()
    {
        ReloadBar = new Panel();
        ReloadBar.AddClass("reload_bar");
        AddChild(ReloadBar);

        ReloadBarTickRight = new Panel();
        ReloadBarTickRight.AddClass("reload_bar_tick_right");
        AddChild(ReloadBarTickRight);
        //ReloadBarTickRight.Style.Opacity = 0f;
    }

    public override void Tick()
    {
        base.Tick();

        if (Player == null || !Player.IsValid)
            return;

        var name = Player.Client.Name;
        NameLabel.Text = name[..Math.Min(name.Length, 14)] + " [" + Player.Level.ToString() + "]";

        var screenPos = Camera2D.Current.WorldToScreen(Player.Position + new Vector2(0f, 1.42f + Player.HeightZ)) * ScaleFromScreen;

        Style.Left = screenPos.x - 150;
        Style.Top = screenPos.y;

        var BAR_WIDTH = 100;
        HpBar.Style.Width = BAR_WIDTH;
        HpBarOverlay.Style.Width = BAR_WIDTH;
        HpBarDelta.Style.Width = BAR_WIDTH;

        var player_health_ratio = Math.Clamp(Player.Health / Player.MaxHp, 0f, 1f);

        var tr = new PanelTransform();
        tr.AddScale(new Vector3(player_health_ratio, 1f, 1f));
        HpBarOverlay.Style.Transform = tr;
        HpBarDelta.Style.Transform = tr;

        //var colors = new List<Color>() { Color.Green, Color.Yellow, Color.Red };
        //var t = 1f - player_health_ratio;
        //float scaledTime = t * 2f;
        //Color oldColor = colors[(int)scaledTime];
        //Color newColor = colors[(int)(scaledTime + 1f)];
        //float newT = scaledTime - MathF.Round(scaledTime);
        //var color = Color.Lerp(oldColor, newColor, newT);

        var color = Lerp3(new Color(0f, 0.75f, 0f), new Color(0.75f, 0.75f, 0f), new Color(1f, 0f, 0f), 1f - player_health_ratio);
        HpBarOverlay.Style.BackgroundColor = color;

        if(ReloadBar != null)
        {
            if(Player.IsReloading)
            {
                ReloadBar.AddClass("showing");
                ReloadBarTickRight.AddClass("showing");

                var progress = Utils.EasePercent(Player.ReloadProgress, EasingType.SineInOut);

                var rtr = new PanelTransform();
                rtr.AddScale(new Vector3(progress, 1f, 1f));
                ReloadBar.Style.Transform = rtr;

                ReloadBarTickRight.Style.Left = Utils.Map(progress, 0f, 1f, 99f, 199f);
            }
            else
            {
                ReloadBar.RemoveClass("showing");
                ReloadBarTickRight.RemoveClass("showing");
            }
        }
    }

    Color Lerp3(Color a, Color b, Color c, float t)
    {
        if (t < 0.5f) // 0.0 to 0.5 goes to a -> b
            return Color.Lerp(a, b, t / 0.5f);
        else // 0.5 to 1.0 goes to b -> c
            return Color.Lerp(b, c, (t - 0.5f) / 0.5f);
    }

    public void SetVisible(bool visible)
    {
        if (visible)
        {
            HpBar.RemoveClass("invisible");
            HpBarOverlay.RemoveClass("invisible");
        }
        else
        {
            HpBar.AddClass("invisible");
            HpBarOverlay.AddClass("invisible");
        }
    }
}
