using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;

namespace Sandbox
{
    public class MainHud : RootPanel
    {
        public static Vector2 MousePos { get; private set; }

        public MainHud()
        {
            Style.PointerEvents = PointerEvents.All;
        }

        public override void Tick()
        {
            base.Tick();

        }

        protected override void OnMouseMove(MousePanelEvent e)
        {
            base.OnMouseMove(e);

            MousePos = e.LocalPosition;

            //Log.Info(e.LocalPosition / Screen.Size);
        }
    }
}
