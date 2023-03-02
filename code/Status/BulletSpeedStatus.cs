using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(3, 0, 1f)]
public class BulletSpeedStatus : Status
{
	public BulletSpeedStatus()
    {
		Title = "Speedy Bullets";
		IconPath = "textures/icons/fast_bullets.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.BulletSpeed, GetMultForLevel(Level), ModifierType.Mult);
        Player.Modify(this, PlayerStat.Recoil, GetRecoilForLevel(Level), ModifierType.Add);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase bullet speed by {0}% but add {1} units of recoil", GetPercentForLevel(Level), PrintRecoilForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase bullet speed by {0}% → {1}% but add {2} → {3} units of recoil", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel), PrintRecoilForLevel(newLevel - 1), PrintRecoilForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		return 1f + 0.35f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 35 * level;
	}

    public float GetRecoilForLevel(int level)
    {
        return 0.75f * level;
    }

    public string PrintRecoilForLevel(int level)
    {
        return string.Format("{0:0.00}", GetRecoilForLevel(level));
    }
}
