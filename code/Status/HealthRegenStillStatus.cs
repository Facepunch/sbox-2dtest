using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class HealthRegenStillStatus : Status
{
	public HealthRegenStillStatus()
    {
		Title = "Meditation";
		IconPath = "textures/icons/health_regen_still.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.HealthRegenStill, GetAmountForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase health regen by {0}/s when you're not moving", GetPrintAmountForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase health regen by {0}/s → {1}/s when you're not moving", GetPrintAmountForLevel(newLevel - 1), GetPrintAmountForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAmountForLevel(int level)
    {
		return 1.4f * level;
    }

	public string GetPrintAmountForLevel(int level)
	{
        return string.Format("{0:0.00}", GetAmountForLevel(level));
    }
}
