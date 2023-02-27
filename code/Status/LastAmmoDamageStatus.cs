using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(9, 0, 1f)]
public class LastAmmoDamageStatus : Status
{
	public LastAmmoDamageStatus()
    {
		Title = "Clutch Shot";
		IconPath = "textures/icons/last_ammo.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.LastAmmoDamageMultiplier, GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Last ammo in the mag does {0}% more damage", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Last ammo in the mag does {0}% → {1}% more damage", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		return 1f + 0.4f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 40 * level;
	}
}
