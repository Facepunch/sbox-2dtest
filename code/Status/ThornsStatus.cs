using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(9, 0, 1f)]
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

		Player.Modify(this, StatType.ThornsPercent, GetAddForLevel(Level), ModifierType.Add);
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
		return 0.25f * level + (level == 9 ? 0.25f : 0f);
    }

	public float GetPercentForLevel(int level)
	{
		return 25 * level + (level == 9 ? 25 : 0);
	}
}
