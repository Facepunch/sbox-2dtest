using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(5, 0, 1f)]
public class DamageReductionStatus : Status
{
	public DamageReductionStatus()
    {
		Title = "Kevlar Nanobots";
		IconPath = "textures/icons/kevlar_nanobot.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, nameof(Player.DamageReductionPercent), GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Reduce damage taken by {0}%", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Reduce damage taken by {0}% → {1}%", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
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
