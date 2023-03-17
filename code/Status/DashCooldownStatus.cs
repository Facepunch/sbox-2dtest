using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(4, 0, 1f)]
public class DashCooldownStatus : Status
{
	public DashCooldownStatus()
    {
		Title = "Quick Lungs";
		IconPath = "textures/icons/dash_cooldown.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.DashCooldown, GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Dash cooldown is {0}% faster", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Dash cooldown is {0}%→{1}% faster", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		return 1f - level * 0.15f;
    }

	public float GetPercentForLevel(int level)
	{
		return 15 * level;
	}
}
