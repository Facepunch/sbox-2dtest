﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

public class CritChanceStatus : Status
{
	public CritChanceStatus()
    {
		Title = "Sharp Bullets";
		IconPath = "textures/icons/crit_chance.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, "CritChance", GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase critical chance by {0}%", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase critical chance by {0}% → {1}%", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 0.1f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 10 * level;
	}
}