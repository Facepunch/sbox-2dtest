using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(5, 0, 1f)]
public class BulletForceStatus : Status
{
	public BulletForceStatus()
    {
		Title = "Bullet Force";
		IconPath = "textures/icons/bullet_force.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.BulletForce, GetMultForLevel(Level), ModifierType.Mult);
        Player.Modify(this, PlayerStat.Recoil, GetRecoilForLevel(Level), ModifierType.Add);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase bullet knockback by {0}% and add {1} units of recoil", GetPercentForLevel(Level), PrintRecoilForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase bullet knockback by {0}%→{1}% and add {2}→{3} units of recoil", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel), PrintRecoilForLevel(newLevel - 1), PrintRecoilForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		return 1f + 0.50f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 50 * level;
	}

    public float GetRecoilForLevel(int level)
    {
        return 0.8f * level;
    }

    public string PrintRecoilForLevel(int level)
    {
        return string.Format("{0:0.00}", GetRecoilForLevel(level));
    }
}
