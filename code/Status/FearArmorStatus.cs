﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(5, 0, 1f, typeof(DashFearStatus))]
public class FearArmorStatus : Status
{
	public FearArmorStatus()
    {
		Title = "Scary Shield";
		IconPath = "textures/icons/fear_armor.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.FearOnMeleeChance, GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("{0}% chance to scare enemy melee attackers", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("{0}% → {1}% chance to scare enemy melee attackers", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 0.2f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 20 * level;
	}
}
