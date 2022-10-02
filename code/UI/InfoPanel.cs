using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Sandbox;

public class InfoPanel : Panel
{
	//public bool IsDirty { get; private set; }

	public Label NameLabel { get; internal set; }
	public Label StatsLabel { get; internal set; }

	public InfoPanel()
	{
		NameLabel = Add.Label("Name", "info_name");
		StatsLabel = Add.Label("Stats", "stats_label");
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
		stats += "DMG: " + player.Dmg + "\n";
		stats += "AMMO : " + player.AmmoCount + " / " + player.MaxAmmoCount + "\n";
		stats += "NUM BULLETS: " + player.NumBullets + "\n";
		stats += "ATTACK SPD: " + player.AttackSpeed + "\n";
		stats += "RELOAD SPD: " + player.ReloadSpeed + "\n";
		stats += "SPREAD: " + player.BulletSpread + "\n";
		stats += "INACCURACY: " + player.BulletInaccuracy + "\n";
		stats += "BULLET SPD: " + player.BulletSpeed + "\n";
		stats += "BULLET LIFETIME: " + player.BulletLifetime + "\n";
		stats += "MOVE SPD: " + player.MoveSpeed + "\n";
		stats += "ATTRACT RANGE: " + player.CoinAttractRange + "\n";
		stats += "ATTRACT POWER: " + player.CoinAttractStrength + "\n";
		stats += "LUCK: " + player.Luck + "\n";
		stats += "SIZE: " + player.Radius + "\n";

		StatsLabel.Text = stats;
	}
}
