using Sandbox.UI.Construct;
using System;
using System.Linq;
using Sandbox.UI;
using Sandbox;

namespace Test2D;

[UseTemplate]
public class Modal : Panel
{
	public Modal()
	{

	}

	public void TestButtonClicked()
    {
		Log.Info("TestButtonClicked!");
    }
}
