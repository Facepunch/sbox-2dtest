using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(3, 0, 1f)]
public class LevelUpMaxHealth : Status
{
	public LevelUpMaxHealth()
    {
		Title = "HP Growth";
		IconPath = "textures/icons/level_up_max_health.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);
	}

    public override string GetDescription(int newLevel)
    {
        return string.Format("Gain +{0} max HP whenever you level up", PrintForLevel(Level));
    }

    public override string GetUpgradeDescription(int newLevel)
    {
        return newLevel > 1 ? string.Format("Gain +{0}→+{1} max HP whenever you level up", PrintForLevel(newLevel - 1), PrintForLevel(newLevel)) : GetDescription(newLevel);
    }

    public override void OnLevelUp()
    {
        Player.AdjustBaseStat(PlayerStat.MaxHp, GetAmountForLevel(Level));
    }

    public float GetAmountForLevel(int level)
    {
        return level * 0.7f;
    }

    public string PrintForLevel(int level)
    {
        return string.Format("{0:0.0}", GetAmountForLevel(level));
    }
}
