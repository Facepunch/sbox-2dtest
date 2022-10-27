using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(9, 0, 111f)]
public class DashStrengthStatus : Status
{
	public DashStrengthStatus()
    {
		Title = "Leg Day";
		IconPath = "textures/icons/dash_strength.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, nameof(Player.DashStrength), GetMultForLevel(Level), ModifierType.Mult);
		Player.Modify(this, nameof(Player.DashInvulnTime), GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("You dash {0}% longer", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("You dash {0}% → {1}% longer", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		return 1f + 0.3f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 30 * level;
	}
}
