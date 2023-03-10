using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(4, 0, 1f)]
public class DamageMinusBulletsStatus : Status
{
	public DamageMinusBulletsStatus()
    {
		Title = "Consolidate";
		IconPath = "textures/icons/damage_minus_bullets.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.BulletDamage, GetDamageMultForLevel(Level), ModifierType.Mult);
        Player.Modify(this, PlayerStat.MaxAmmoCount, -GetAmmoForLevel(Level), ModifierType.Add);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase bullet damage by {0}% but shoot {1} less ammo per mag", GetDamagePercentForLevel(Level), GetAmmoForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase bullet damage by {0}% → {1}% but shoot {2} → {3} less ammo per mag", GetDamagePercentForLevel(newLevel - 1), GetDamagePercentForLevel(newLevel), GetAmmoForLevel(newLevel - 1), GetAmmoForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetDamageMultForLevel(int level)
    {
		return 1f + 0.20f * level;
    }

    public float GetDamagePercentForLevel(int level)
	{
		return 20 * level;
	}

    public float GetAmmoForLevel(int level)
    {
        return level;
    }
}
