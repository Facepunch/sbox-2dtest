using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f, typeof(FireIgniteStatus), typeof(DashFireStatus))]
public class FreezeBurnStatus : Status
{
	public FreezeBurnStatus()
    {
		Title = "FreezerBurn";
		IconPath = "textures/icons/freeze_burn.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.FreezeFireDamageMultiplier, GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Deal {0}% more fire damage to frozen enemies", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Deal {0}% → {1}% more fire damage to frozen enemies", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		return 1f + (0.40f * level);
    }

	public float GetPercentForLevel(int level)
	{
		return 40 * level;
	}
}
