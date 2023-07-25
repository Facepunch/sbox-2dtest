using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Test2D;

public class DeathPanel : Panel
{
	public DeathModal DeathModal { get; private set; }

	public DeathPanel()
	{
		DeathModal = new DeathModal(this);
		DeathModal.AddClass("death_panel");
		AddChild(DeathModal);
	}

	public void OnDeathButtonClicked()
	{
		if (Sandbox.Game.LocalClient.IsListenServerHost)
        {
			MyGame.RestartCmd();
			Delete();
			MyGame.Current.Hud.DeathPanel = null;
		}
	}

	async Task SubmitStats()
	{
		await Sandbox.Services.Stats.FlushAsync();

		await Sandbox.Services.Stats.LocalPlayer.Refresh();

		//
		// Stats from the start of the game vs stats
		// at the end of the game. We can use this to 
		// show +7575 kills, +34543543 xp etc.
		//

		var now = Sandbox.Services.Stats.LocalPlayer;
		var then = MyGame.StartStats;

		var addedKills = now["kills"].Value - then["kills"].Value;
		var destroyedCrates = now["crates"].Value - then["crates"].Value;

        Log.Info( $"You killed {addedKills:n0} zombies this game!" );
        Log.Info( $"You opened {destroyedCrates:n0} crates!" );
	}


    public async Task FailureAsync()
    {
		await SubmitStats();

        //TimeSpan t = TimeSpan.FromSeconds(MyGame.Current.ElapsedTime.Relative);
        //string timeText = t.TotalSeconds > 3600 ? t.ToString(@"hh\:mm\:ss") : t.ToString(@"mm\:ss");

    }

    public async Task VictoryAsync()
    {
        DeathModal.TextLabel.Text = "Victory!";

        await SubmitStats();

        //TimeSpan t = TimeSpan.FromSeconds(MyGame.Current.ElapsedTime.Relative);
        //string timeText = t.TotalSeconds > 3600 ? t.ToString(@"hh\:mm\:ss") : t.ToString(@"mm\:ss");
    }
}

public class DeathModal : Panel
{
	public DeathPanel DeathPanel { get; set; }
	public Label TextLabel { get; set; }

	public DeathModal(DeathPanel deathPanel)
    {
		DeathPanel = deathPanel;

		TextLabel = new Label();
		TextLabel.AddClass("death_label");
		TextLabel.Text = "You're Dead...";
		//titleLabel.Text = "Title";
		AddChild(TextLabel);

		var button = Add.Panel("death_button");    
        button.AddEventListener("onclick", () => DeathPanel.OnDeathButtonClicked());
    
        Label button_label = new Label();
        button_label.AddClass("death_button_label");
        
		if (Input.UsingController)
		{
            button_label.Text = "Press Duck to Restart";
        }
		else
		{
        button_label.Text = "Retry";
		}
        
        button.AddChild(button_label);

        if (!Sandbox.Game.LocalClient.IsListenServerHost)
        {
			button.AddClass("disabled");
			button_label.Text += " (Host only)";
		}
	}

    public override void Tick()
    {
        base.Tick();
		
        if (Input.Down("Duck"))
        {
            DeathPanel.OnDeathButtonClicked();
        }
    }
}
