﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class XpAttractRangeStatus : Status
{
	public XpAttractRangeStatus()
    {
		Title = "XP Radar";
		IconPath = "textures/icons/xp_range.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.CoinAttractRange, GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase XP attract range by {0}%", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase XP attract range by {0}%→{1}%", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		return 1f + 0.4f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 40 * level;
	}
}
