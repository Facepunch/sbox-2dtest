using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(9, 0, 1f, typeof(FireIgniteStatus))]
public class FireDamageStatus : Status
{
	public FireDamageStatus()
    {
		Title = "Hotter Fires";
		IconPath = "textures/icons/fire_damage.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, nameof(Player.FireSpreadChance), GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase fire damage by {0}%", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase fire damage by {0}% → {1}%", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 0.15f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 15 * level;
	}
}
