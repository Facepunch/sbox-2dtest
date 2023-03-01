using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class HealthRegenStatus : Status
{
	public HealthRegenStatus()
    {
		Title = "Self-Heal";
		IconPath = "textures/icons/health_regen.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.HealthRegen, GetAmountForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase health regen by {0}/s", GetPrintAmountForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase health regen by {0}/s → {1}/s", GetPrintAmountForLevel(newLevel - 1), GetPrintAmountForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAmountForLevel(int level)
    {
		return 0.3f * level;
    }

	public string GetPrintAmountForLevel(int level)
	{
		return GetAmountForLevel(level).ToString("#.##");
	}
}
