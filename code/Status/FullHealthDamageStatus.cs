using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class FullHealthDamageStatus : Status
{
	public FullHealthDamageStatus()
    {
		Title = "Perfectionist";
		IconPath = "textures/icons/blank_icon.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.FullHealthDamageMultiplier, GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Deal {0}% more damage when you have full HP", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Deal {0}% → {1}% more damage when you have full HP", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 0.30f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 30 * level;
	}
}
