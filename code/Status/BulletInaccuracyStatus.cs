using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(3, 0, 1f)]
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

		Player.Modify(this, PlayerStat.BulletInaccuracy, GetInaccuracyMultForLevel(Level), ModifierType.Mult);
        Player.Modify(this, PlayerStat.BulletSpeed, GetSpeedMultForLevel(Level), ModifierType.Mult);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("{0}% less bullet inaccuracy and {1}% faster bullets", GetInaccuracyPercentForLevel(Level), GetSpeedPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("{0}%→{1}% less bullet inaccuracy and {1}%→{2}% faster bullets", GetInaccuracyPercentForLevel(newLevel - 1), GetInaccuracyPercentForLevel(newLevel), GetSpeedPercentForLevel(newLevel - 1), GetSpeedPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetInaccuracyMultForLevel(int level)
    {
		return level == 1 ? 0.70f : (level == 2 ? 0.40f : 0f);
	}

	public float GetInaccuracyPercentForLevel(int level)
	{
		return level == 1 ? 30 : (level == 2 ? 60 : 100);
	}

    public float GetSpeedMultForLevel(int level)
    {
        return 1f + 0.15f * level;
    }

    public float GetSpeedPercentForLevel(int level)
    {
        return 15 * level;
    }
}
