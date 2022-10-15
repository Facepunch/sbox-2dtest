using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Test2D;

public class InfoPanel : Panel
{
	//public bool IsDirty { get; private set; }

	public Label NameLabel { get; internal set; }
	public Label StatsLabel { get; internal set; }

	public DashContainer DashContainer { get; set; }

	public InfoPanel()
	{
		NameLabel = Add.Label("Name", "info_name");
		StatsLabel = Add.Label("Stats", "stats_label");

		var outerDashContainter = new Panel();
		outerDashContainter.AddClass("info_dash_outer_container");
		AddChild(outerDashContainter);

		DashContainer = new DashContainer();
		DashContainer.AddClass("info_dash_container");
		outerDashContainter.AddChild(DashContainer);
	}

	public override void Tick()
	{
		base.Tick();

		var player = MyGame.Current.LocalPlayer;
		if (player == null)
			return;

		NameLabel.Text = player.Client.Name;

		string stats = "\n";

		stats += "LEVEL: " + player.Level + "\n";
		stats += "XP: " + player.ExperienceCurrent + " / " + player.ExperienceRequired + "\n\n";

		stats += "HP: " + player.Health.ToString("#.#") + " / " + player.MaxHp + "\n";
		stats += "HEALTH REGEN: " + player.HealthRegen + "/s" + "\n";
		stats += "DMG: " + player.BulletDamage + "\n";
		stats += "AMMO : " + player.AmmoCount + " / " + player.MaxAmmoCount + "\n";
		stats += "NUM BULLETS: " + player.NumBullets + "\n";
		stats += "ATTACK SPD: " + player.AttackSpeed + "\n";
		stats += "RELOAD SPD: " + player.ReloadSpeed + "\n";
		stats += "SPREAD: " + player.BulletSpread + "\n";
		stats += "INACCURACY: " + player.BulletInaccuracy + "\n";
		stats += "BULLET SPD: " + player.BulletSpeed + "\n";
		stats += "BULLET LIFETIME: " + player.BulletLifetime + "\n";
		stats += "BULLET KNOCKBACK: " + player.BulletForce + "\n";
		stats += "PIERCING: " + (int)MathF.Round(player.BulletNumPiercing) + "\n";
		stats += "MOVE SPD: " + player.MoveSpeed.ToString("#.##") + "\n";
		stats += "ATTRACT RANGE: " + player.CoinAttractRange + "\n";
		stats += "ATTRACT POWER: " + player.CoinAttractStrength + "\n";
		stats += "LUCK: " + player.Luck + "\n";
		stats += "SIZE: " + player.Radius + "\n";
		stats += "CRIT CHANCE: " + player.CritChance + "\n";
		stats += "CRIT MULTIPLIER: " + player.CritMultiplier + "\n";
		stats += "NUM CHOICES: " + player.NumUpgradeChoices + "\n";

		StatsLabel.Text = stats;
	}
}

public class DashContainer : Panel
{
	private List<DashIcon> _icons = new List<DashIcon>();

	public DashContainer()
	{
		
	}

	public override void Tick()
	{
		base.Tick();

		var player = MyGame.Current.LocalPlayer;
		if (player == null) 
			return;

		int numDashes = (int)MathF.Round(player.NumDashes);
		if (_icons.Count != numDashes)
			Refresh(numDashes);

		for(int i = 0; i < _icons.Count; i++)
        {
			var icon = _icons[i];

			var amount = i == player.NumDashesAvailable ? player.DashRechargeProgress : (i < player.NumDashesAvailable ? 1f : 0f);
			var xform = new PanelTransform();
			xform.AddScale(new Vector3(amount, 1f, 1f));
			icon.OverlayPanel.Style.Transform = xform;

			if (i < player.NumDashesAvailable)
                icon.OverlayPanel.RemoveClass("recharge");
            else
                icon.OverlayPanel.AddClass("recharge");
		}
	}

	public void Refresh(int numDashes)
	{
		foreach (var icon in _icons)
			icon.Delete();
		_icons.Clear();

		for (int i = 0; i < numDashes; i++)
        {
			var icon = new DashIcon();
			icon.AddClass("info_dash_icon");
			AddChild(icon);
			_icons.Add(icon);
		}
	}
}

public class DashIcon : Panel
{
	public Panel OverlayPanel { get; set; }

	public DashIcon()
	{
		OverlayPanel = new Panel();
		OverlayPanel.AddClass("dash_icon_overlay");
		AddChild(OverlayPanel);

		var label = new Label();
		label.AddClass("dash_icon_label");
		label.Text = "DASH";
		AddChild(label);
	}

	protected override void OnMouseOver(MousePanelEvent e)
	{
		base.OnMouseOver(e);

		Tippy.Create(this, Tippy.Pivots.TopLeft).WithContent("Dash", "Press SPACE to use", 0);
	}
}