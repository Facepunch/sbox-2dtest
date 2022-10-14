using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(9, 0, 1f)]
public class BulletInaccuracyStatus : Status
{
	public BulletInaccuracyStatus()
    {
		Title = "Steady Hand";
		IconPath = "textures/icons/bullet_accuracy.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, "BulletInaccuracy", GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("{0}% less inaccuracy", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("{0}% → {1}% less inaccuracy", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		return 1f - 0.25f * level;
    }

	public float GetPercentForLevel(int level)
	{
		return 25 * level;
	}
}
