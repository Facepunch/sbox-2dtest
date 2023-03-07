using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f, typeof(FreezeShootStatus), typeof(FreezeArmorStatus))]
public class FreezeShardsStatus : Status
{
	public FreezeShardsStatus()
    {
		Title = "Frozen Shards";
		IconPath = "textures/icons/freeze_shards.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

    public override void Refresh()
    {
        Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.FrozenShardsNum, GetAddForLevel(Level), ModifierType.Add);
    }

    public override string GetDescription(int newLevel)
    {
        return string.Format("Enemies you freeze release {0} bullet when they die", GetPercentForLevel(Level));
    }

    public override string GetUpgradeDescription(int newLevel)
    {
        return newLevel > 1 ? string.Format("Enemies you freeze release up to {0} → {1} bullets when they die", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
    }

    public float GetAddForLevel(int level)
    {
        return 1f * level;
    }

    public float GetPercentForLevel(int level)
    {
        return 1 * level;
    }
}
