using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 111f)]
public class CrateChanceStatus : Status
{
    public CrateChanceStatus()
    {
        Title = "More Crates";
        IconPath = "textures/icons/blank_icon.png";
    }

    public override void Init(PlayerCitizen player)
    {
        base.Init(player);
    }

    public override void Refresh()
    {
        Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.CrateChanceAdditional, GetAddForLevel(Level), ModifierType.Add); ;
    }

    public override string GetDescription(int newLevel)
    {
        return string.Format("{0}% greater chance to spawn crates", GetPercentForLevel(Level));
    }

    public override string GetUpgradeDescription(int newLevel)
    {
        return newLevel > 1 ? string.Format("{0}% → {1}% greater chance to spawn crates", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
    }

    public float GetAddForLevel(int level)
    {
        return level * 0.05f;
    }

    public float GetPercentForLevel(int level)
    {
        return level * 5;
    }
}
