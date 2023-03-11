using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class LowHealthDamageStatus : Status
{
	public LowHealthDamageStatus()
    {
		Title = "Bloody Rage";
		IconPath = "textures/icons/low_health_damage.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.LowHealthDamageMultiplier, GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Deal up to {0}% more damage when you have less HP", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Deal up to {0}% → {1}% more damage when you have less HP", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		return 1f + 0.30f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 30 * level;
	}
}
