using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(9, 0, 1f, typeof(BulletSpeedStatus), typeof(DamageStatus))]
public class BulletForceStatus : Status
{
	public BulletForceStatus()
    {
		Title = "Strong Bullets";
		IconPath = "textures/icons/bullet_force.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, "BulletForce", GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase bullet knockback by {0}%", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase bullet knockback by {0}% → {1}%", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		return 1f + 0.25f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 25 * level;
	}
}
