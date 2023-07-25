using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Test2D;

public partial class Nametag : Panel
{
    public PlayerCitizen Player { get; set; }

    public Label NameLabel { get; set; }
    public Label LevelLabel { get; set; }

    public Panel HpBar { get; set; }
    public Panel HpBarOverlay { get; set; }
    public Panel HpBarDelta { get; set; }
    public Panel ReloadBar { get; set; }

    public Nametag()
    {
        StyleSheet.Load("/UI/Nametag.scss");

        NameLabel = new Label();
        NameLabel.AddClass("name");
        AddChild(NameLabel);

        LevelLabel = new Label();
        LevelLabel.AddClass("level");
        AddChild(LevelLabel);

        HpBar = new Panel();
        HpBar.AddClass("hpbar");
        AddChild(HpBar);

        HpBarDelta = new Panel();
        HpBarDelta.AddClass("hpbardelta");
        HpBar.AddChild(HpBarDelta);

        HpBarOverlay = new Panel();
        HpBarOverlay.AddClass("hpbaroverlay");
        HpBar.AddChild(HpBarOverlay);
    }

    public void AddReloadBar()
    {
        ReloadBar = new Panel();
        ReloadBar.AddClass("reload_bar");
        AddChild(ReloadBar);
    }

    public override void Tick()
    {
        base.Tick();

        if (LevelLabel == null || Player == null || !Player.IsValid || NameLabel == null || HpBar == null || HpBarOverlay == null || HpBarDelta == null)
            return;

        var name = Player.Client.Name;
        NameLabel.Text = name.Truncate( 12, ".." );
        LevelLabel.Text = Player.Level.ToString();

        var screenPos = Camera2D.Current.WorldToScreen((Vector2)Player.SceneObject.Position + new Vector2(0f, 1.42f + Player.HeightZ)) * ScaleFromScreen;

        Style.Left = screenPos.x;
        Style.Top = screenPos.y;

        var player_health_ratio = Math.Clamp(Player.Health / Player.Stats[PlayerStat.MaxHp], 0f, 1f);

        HpBarOverlay.Style.Width = Length.Fraction(player_health_ratio);
        HpBarDelta.Style.Width = Length.Fraction(player_health_ratio);

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

                var progress = Utils.EasePercent(Player.ReloadProgress, EasingType.CubicInOut);

                ReloadBar.Style.Width = Length.Fraction(progress);
            }
            else
            {
                ReloadBar.RemoveClass("showing");
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
            HpBar?.RemoveClass("invisible");
            HpBarOverlay?.RemoveClass("invisible");
        }
        else
        {
            HpBar?.AddClass("invisible");
            HpBarOverlay?.AddClass("invisible");
        }
    }
}
