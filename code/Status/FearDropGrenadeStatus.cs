using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f, typeof(DashFearStatus), typeof(GrenadeFearStatus))]
public class FearDropGrenadeStatus : Status
{
    public FearDropGrenadeStatus()
    {
        Title = "Bomb Curse";
        IconPath = "textures/icons/blank_icon.png";
    }

    public override void Init(PlayerCitizen player)
    {
        base.Init(player);
    }

    public override void Refresh()
    {
        Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.FearDropGrenadeChance, GetAddForLevel(Level), ModifierType.Add); ;
    }

    public override string GetDescription(int newLevel)
    {
        return string.Format("Enemies you scare have a {0}% chance to drop a grenade on death", GetPercentForLevel(Level));
    }

    public override string GetUpgradeDescription(int newLevel)
    {
        return newLevel > 1 ? string.Format("Enemies you scare have a {0}% → {1}% chance to drop a grenade on death", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
    }

    public float GetAddForLevel(int level)
    {
        return level * 0.07f;
    }

    public float GetPercentForLevel(int level)
    {
        return level * 7;
    }
}
