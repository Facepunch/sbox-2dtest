using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox;

namespace Test2D;

public class MainHud : RootPanel
{
    public static Vector2 MousePos { get; private set; }

    public Label TestLabel { get; set; }
    public Panel TestPanel { get; set; }
    public Button TestButton { get; set; }

    public MainHud()
    {
        Style.PointerEvents = PointerEvents.All;

        StyleSheet.Load("Resource/styles/hud.scss");
        SetTemplate("Resource/templates/hud.html");

        TestLabel = Add.Label("test", "testlabel");
        TestPanel = Add.Panel("testpanel");
        TestButton = TestPanel.Add.Button("T", () => { ButtonClicked(); });

        AddChild<Modal>();
    }

    public override void Tick()
    {
        base.Tick();

        //TestLabel.Text = "test";
    }

    protected override void OnMouseMove(MousePanelEvent e)
    {
        base.OnMouseMove(e);

        MousePos = e.LocalPosition;

        //Log.Info(e.LocalPosition / Screen.Size);
    }

    public void ButtonClicked()
    {
        Log.Info("ButtonClicked");
    }
}
