using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f, typeof(DashFearStatus), typeof(GrenadeFearStatus))]
public class FearLifetimeStatus : Status
{
	public FearLifetimeStatus()
    {
		Title = "Long Nightmares";
		IconPath = "textures/icons/blank_icon.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.FearLifetime, GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("You scare enemies for {0}% longer", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("You scare enemies for {0}% → {1}% longer", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		return 1f + 0.5f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 50 * level;
	}
}
