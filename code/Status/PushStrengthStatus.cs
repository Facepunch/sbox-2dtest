using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class PushStrengthStatus : Status
{
	public PushStrengthStatus()
    {
		Title = "Muscles";
		IconPath = "textures/icons/muscles.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.PushStrength, GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Push past enemies {0}% more", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Push past enemies {0}% → {1}% more", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		return 1f + 0.65f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 65 * level;
	}
}
