using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(3, 0, 1f)]
public class MoreRerollsStatus : Status
{
	public MoreRerollsStatus()
    {
		Title = "More Rerolls";
		IconPath = "textures/icons/blank_icon.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.NumRerollsPerLevel, GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Gain {0} addition reroll each level", GetAddForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Gain {0} → {1} addition rerolls each level", GetAddForLevel(newLevel - 1), GetAddForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 1f * level;
    }
}
