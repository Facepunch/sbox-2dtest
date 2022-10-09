using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Test2D;

[UseTemplate]
public partial class TimerPanel : Panel
{
    public Label TimerLabel { get; set; }

    public TimeSince ElapsedTime { get; set; }

    public TimerPanel()
    {
        ElapsedTime = 0f;
    }

    public override void Tick()
    {
        base.Tick();

        if (MyGame.Current.IsGameOver)
            return;

        TimeSpan t = TimeSpan.FromSeconds(ElapsedTime.Relative);
        TimerLabel.Text = t.TotalSeconds > 3600 ? t.ToString(@"hh\:mm\:ss") : t.ToString(@"mm\:ss");
    }

    public void SetVisible(bool visible)
    {
        if (visible)
            RemoveClass("invisible");
        else
            AddClass("invisible");
    }

    public void Reset()
    {
        ElapsedTime = 0f;
    }
}
