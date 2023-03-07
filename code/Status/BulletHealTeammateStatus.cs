using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 111f)]
public class BulletHealTeammateStatus : Status
{
	public BulletHealTeammateStatus()
    {
		Title = "Healing Darts";
		IconPath = "textures/icons/blank_icon.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.BulletHealTeammateAmount, GetAddForLevel(Level), ModifierType.Add);
        Player.Modify(this, PlayerStat.OverallDamageMultiplier, GetDamageMultForLevel(Level), ModifierType.Mult);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Your bullets heal allies for {0} HP but you deal {1}% less damage", GetPrintForLevel(Level), GetDamagePercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Your bullets heal allies for {0} → {1} HP but you deal {2}% → {3}% less damage", GetPrintForLevel(newLevel - 1), GetPrintForLevel(newLevel), GetDamagePercentForLevel(newLevel - 1), GetDamagePercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 0.75f * level;
    }

	public string GetPrintForLevel(int level)
	{
        return string.Format("{0:0.00}", GetAddForLevel(level));
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
