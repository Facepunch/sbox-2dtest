using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Test2D;

[UseTemplate]
public partial class EnemyNametag : Panel
{
    public Enemy Enemy { get; set; }

    public Panel HpBar { get; set; }
    public Panel HpBarOverlay { get; set; }
    public Panel HpBarDelta { get; set; }

    public override void Tick()
    {
        base.Tick();

        if (Enemy == null || !Enemy.IsValid)
            return;

        var screenPos = MyGame.Current.MainCamera.WorldToScreen(Enemy.Position + new Vector2(0f, 1.66f + Enemy.HeightZ)) * ScaleFromScreen;

        Style.Left = screenPos.x - 150;
        Style.Top = screenPos.y;

        var BAR_WIDTH = 100;
        HpBar.Style.Width = BAR_WIDTH;
        HpBarOverlay.Style.Width = BAR_WIDTH;
        HpBarDelta.Style.Width = BAR_WIDTH;

        var health_ratio = Math.Clamp(Enemy.Health / Enemy.MaxHealth, 0f, 1f);

        var tr = new PanelTransform();
        tr.AddScale(new Vector3(health_ratio, 1f, 1f));
        HpBarOverlay.Style.Transform = tr;
        HpBarDelta.Style.Transform = tr;

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
            HpBarOverlay.RemoveClass("invisible");
        }
        else
        {
            HpBar.AddClass("invisible");
            HpBarOverlay.AddClass("invisible");
        }
    }
}
