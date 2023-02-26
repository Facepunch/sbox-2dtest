using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(6, 0, 1f)]
public class FreezeShootStatus : Status
{
	public FreezeShootStatus()
    {
		Title = "Chilly Bullets";
		IconPath = "textures/icons/freeze_shoot.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, StatType.ShootFreezeChance, GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("{0}% chance for your bullets to freeze on hit", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("{0}% → {1}% chance for your bullets to freeze on hit", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 0.15f * level + (level == 6 ? 0.1f : 0f);
    }

	public float GetPercentForLevel(int level)
	{
		return 15 * level + (level == 6 ? 10 : 0);
	}
}
