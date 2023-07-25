using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Test2D;

public partial class EnemyNametag : Panel
{
    public Enemy Enemy { get; set; }

    public Panel HpBar { get; set; }
    public Panel HpBarOverlay { get; set; }
    public Panel HpBarDelta { get; set; }
    public Label BossNameLabel { get; set; }

    public EnemyNametag()
    {
        StyleSheet.Load("/UI/EnemyNametag.scss");

        HpBar = Add.Panel("hpbar");
        HpBarDelta = HpBar.Add.Panel("hpbardelta");
        HpBarOverlay = HpBar.Add.Panel("hpbaroverlay");

        BossNameLabel = new Label();
        BossNameLabel.Text = "BOSS";
        HpBar.AddChild(BossNameLabel);
    }

    public override void Tick()
    {
        base.Tick();

        if (Enemy == null || !Enemy.IsValid)
            return;

        var health_ratio = Math.Clamp(Enemy.Health / Enemy.MaxHealth, 0f, 1f);

        HpBarOverlay.Style.Width = Length.Fraction(health_ratio);
        HpBarDelta.Style.Width = Length.Fraction(health_ratio);

        var color = Lerp3(new Color(0f, 0.75f, 0f), new Color(0.75f, 0.75f, 0f), new Color(1f, 0f, 0f), 1f - health_ratio);
        HpBarOverlay.Style.BackgroundColor = color;
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
        }
        else
        {
            HpBar.AddClass("invisible");
        }
    }
}
