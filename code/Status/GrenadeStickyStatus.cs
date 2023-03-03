using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(1, 0, 0.9f, typeof(ExplosionDamageStatus))]
public class GrenadeStickyStatus : Status
{
    public GrenadeStickyStatus()
    {
        Title = "Sticky Bombs";
        IconPath = "textures/icons/blank_icon.png";
    }

    public override void Init(PlayerCitizen player)
    {
        base.Init(player);
    }

    public override void Refresh()
    {
        Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.GrenadeStickyPercent, GetAddForLevel(Level), ModifierType.Add);
        Player.Modify(this, PlayerStat.MoveSpeed, GetMultForLevel(Level), ModifierType.Mult);
    }

    public override string GetDescription(int newLevel)
    {
        return string.Format("Your grenades are attracted to targets but you have 40% slower move speed");  
    }

    public override string GetUpgradeDescription(int newLevel)
    {
        return newLevel > 1 ? string.Format("Your grenades are attracted to targets but you have 40% slower move speed") : GetDescription(newLevel);
    }

    public float GetAddForLevel(int level)
    {
        return 1f;
    }

    public float GetMultForLevel(int level)
    {
        return 1f - 0.4f * level;
    }

    public float GetPercentForLevel(int level)
    {
        return 40 * level;
    }
}
