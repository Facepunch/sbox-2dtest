using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class ThornsStatus : Status
{
	public ThornsStatus()
    {
		Title = "Thorns";
		IconPath = "textures/icons/thorns.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.ThornsPercent, GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Reflect {0}% of melee damage you take", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Reflect {0}% → {1}% of melee damage you take", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 0.40f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 40 * level;
	}
}
