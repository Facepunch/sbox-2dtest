using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(1, 0, 0.8f, typeof(ExplosionDamageStatus))]
public class GrenadeCritStatus : Status
{
    public GrenadeCritStatus()
    {
        Title = "Critical Grenades";
        IconPath = "textures/icons/blank_icon.png";
    }

    public override void Init(PlayerCitizen player)
    {
        base.Init(player);
    }

    public override void Refresh()
    {
        Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.GrenadesCanCrit, GetAddForLevel(Level), ModifierType.Add);
    }

    public override string GetDescription(int newLevel)
    {
        return string.Format("Your grenades can crit but your bullets can't");  
    }

    public override string GetUpgradeDescription(int newLevel)
    {
        return newLevel > 1 ? string.Format("Your grenades can crit but your bullets can't") : GetDescription(newLevel);
    }

    public float GetAddForLevel(int level)
    {
        return 1f;
    }
}
