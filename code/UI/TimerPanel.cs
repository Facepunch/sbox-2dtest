using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Test2D;

public partial class TimerPanel : Panel
{
    public Label TimerLabel { get; set; }

    public TimerPanel()
    {
        StyleSheet.Load("/UI/TimerPanel.scss");
        TimerLabel = Add.Label("timer_label");
    }

    public override void Tick()
    {
        base.Tick();

        if (MyGame.Current.IsGameOver)
            return;

        TimeSpan t = TimeSpan.FromMinutes( 15 ) - TimeSpan.FromSeconds(MyGame.Current.ElapsedTime.Relative);

        SetClass( "invisible", t.TotalMinutes < 0 );

        if (TimerLabel != null)
            TimerLabel.Text = t.TotalSeconds > 3600 ? t.ToString(@"hh\:mm\:ss") : t.ToString(@"mm\:ss");
    }
}
