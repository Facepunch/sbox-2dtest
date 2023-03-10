using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class DamageStatus : Status
{
	public DamageStatus()
    {
		Title = "Bigger Bullets";
		IconPath = "textures/icons/bigger_bullets.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.BulletDamage, GetDamageMultForLevel(Level), ModifierType.Mult);
        Player.Modify(this, PlayerStat.BulletSpeed, GetSpeedMultForLevel(Level), ModifierType.Mult);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase bullet damage by {0}% and decrease bullet speed by {1}%", GetDamagePercentForLevel(Level), GetSpeedPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase bullet damage by {0}% → {1}% and decrease bullet speed by {2}% → {3}%", GetDamagePercentForLevel(newLevel - 1), GetDamagePercentForLevel(newLevel), GetSpeedPercentForLevel(newLevel - 1), GetSpeedPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetDamageMultForLevel(int level)
    {
		return 1f + 0.12f * level;
    }

    public float GetDamagePercentForLevel(int level)
    {
        return 12 * level;
    }

    public float GetSpeedMultForLevel(int level)
    {
        return 1f - 0.1f * level;
    }

    public float GetSpeedPercentForLevel(int level)
    {
        return 10 * level;
    }
}
