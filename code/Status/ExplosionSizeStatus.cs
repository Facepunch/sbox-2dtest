using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f, typeof(GrenadeShootReloadStatus))]
public class ExplosionSizeStatus : Status
{
    public ExplosionSizeStatus()
    {
        Title = "Bigger Booms";
        IconPath = "textures/icons/blank_icon.png";
    }

    public override void Init(PlayerCitizen player)
    {
        base.Init(player);
    }

    public override void Refresh()
    {
        Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.ExplosionSizeMultiplier, GetMultForLevel(Level), ModifierType.Mult); ;
    }

    public override string GetDescription(int newLevel)
    {
        return string.Format("Increase the size of your explosions by {0}%", GetPercentForLevel(Level));
    }

    public override string GetUpgradeDescription(int newLevel)
    {
        return newLevel > 1 ? string.Format("Increase the size of your explosions by {0}% → {1}%", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
    }

    public float GetMultForLevel(int level)
    {
        return 1f + 0.2f * level;
    }

    public float GetPercentForLevel(int level)
    {
        return 20 * level;
    }
}
