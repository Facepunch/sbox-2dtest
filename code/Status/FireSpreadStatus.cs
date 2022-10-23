using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(9, 0, 1f, typeof(FireIgniteStatus))]
public class FireSpreadStatus : Status
{
	public FireSpreadStatus()
    {
		Title = "Wild Fire";
		IconPath = "textures/icons/fire_spread.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, nameof(Player.FireSpreadChance), GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("{0}% chance for your ignited fires to spread on touch", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("{0}% → {1}% chance for your ignited fires to spread on touch", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 0.1f * level + (level == 9 ? 0.1f : 0f);
    }

	public float GetPercentForLevel(int level)
	{
		return 10 * level + (level == 10 ? 10 : 0);
	}
}
