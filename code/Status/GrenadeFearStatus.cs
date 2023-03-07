using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(4, 0, 1f, typeof(GrenadeShootReloadStatus), typeof(FearDropGrenadeStatus))]
public class GrenadeFearStatus : Status
{
    public GrenadeFearStatus()
    {
        Title = "Terrorism";
        IconPath = "textures/icons/grenade_fear.png";
    }

    public override void Init(PlayerCitizen player)
    {
        base.Init(player);
    }

    public override void Refresh()
    {
        Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.GrenadeFearChance, GetAddForLevel(Level), ModifierType.Add); ;
    }

    public override string GetDescription(int newLevel)
    {
        return string.Format("Your grenades have a {0}% chance to scare enemies they hurt", GetPercentForLevel(Level));
    }

    public override string GetUpgradeDescription(int newLevel)
    {
        return newLevel > 1 ? string.Format("Your grenades have a {0}% → {1}% chance to scare enemies they hurt", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
    }

    public float GetAddForLevel(int level)
    {
        return level == 4 ? 1f : 0.3f * level;
    }

    public float GetPercentForLevel(int level)
    {
        return level == 4 ? 100 : 30 * level;
    }
}
