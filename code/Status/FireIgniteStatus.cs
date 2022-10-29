using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(9, 0, 1f)]
public class FireIgniteStatus : Status
{
	public FireIgniteStatus()
    {
		Title = "Burning Bullets";
		IconPath = "textures/icons/burning_bullets.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, nameof(Player.ShootFireIgniteChance), GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("{0}% chance for your bullets to ignite on hit", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("{0}% → {1}% chance for your bullets to ignite on hit", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAddForLevel(int level)
    {
		return 0.1f * level + (level == 9 ? 0.1f : 0f);
    }

	public float GetPercentForLevel(int level)
	{
		return 10 * level + (level == 9 ? 10 : 0);
	}
}
