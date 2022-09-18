using Sandbox.UI.Construct;
using System;
using System.Linq;
using Sandbox.UI;

namespace Sandbox;

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
