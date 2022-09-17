using Sandbox.UI;
using Sandbox.UI.Construct;

public class MyWorldPanel : WorldPanel
{
    public MyWorldPanel()
    {
        //StyleSheet.Load("/UI/MyWorldPanel.scss");
        Add.Label("hello world");
    }
}