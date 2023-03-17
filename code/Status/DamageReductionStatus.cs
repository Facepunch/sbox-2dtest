using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class DamageReductionStatus : Status
{
	public DamageReductionStatus()
    {
		Title = "Kevlar Nanobots";
		IconPath = "textures/icons/kevlar_nanobot.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.DamageReductionPercent, GetAddForLevel(Level), ModifierType.Add);
        Player.Modify(this, PlayerStat.OverallDamageMultiplier, GetDamageMultForLevel(Level), ModifierType.Mult);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Reduce damage taken by {0}% but reduce damage you deal by {1}%", GetPercentForLevel(Level), GetDamagePercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Reduce damage taken by {0}%→{1}% but reduce damage you deal by {2}%→{3}%", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel), GetDamagePercentForLevel(newLevel - 1), GetDamagePercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 0.1f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 10 * level;
	}

    public float GetDamageMultForLevel(int level)
    {
        return 1f - 0.05f * level;
    }

    public float GetDamagePercentForLevel(int level)
    {
        return 5 * level;
    }
}
