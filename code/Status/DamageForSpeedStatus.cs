using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class DamageForSpeedStatus : Status
{
	public DamageForSpeedStatus()
    {
		Title = "Momentum";
		IconPath = "textures/icons/damage_for_speed.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.DamageForSpeed, GetAmountForLevel(Level), ModifierType.Add);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Bullets deal {0} more damage for each metre per second of your speed", GetPrintAmountForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
	{
		return newLevel > 1 ? string.Format("Bullets deal {0}→{1} more damage for each metre per second of your speed", GetPrintAmountForLevel(newLevel - 1), GetPrintAmountForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAmountForLevel(int level)
	{
		return level * 0.75f;
	}

    public string GetPrintAmountForLevel(int level)
    {
		return string.Format("{0:0.0}", GetAmountForLevel(level));
    }
}
